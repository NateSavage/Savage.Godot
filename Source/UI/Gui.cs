#define LogLoadingMessages
#nullable enable

    using Godot;

    using System;
    using System.Reflection;
    using System.Linq;
    using System.Collections.Generic;

    using Savage.GD;
    using Savage.UI;

    namespace Savage;

    /// <summary> Singleton handle into which you can request GUI things happen. </summary>
    public static class Gui {
        /// <summary> Handle into the Godot scene. </summary>
        static Node node;

        /// <summary> Contains one copy of each <see cref="GuiPage"/> in the assembly for instancing. </summary>
        static Dictionary<System.Type, PackedScene> packedGUIObjects;

        /// <summary> Currently open <see cref="GUIPage"/> instances where <see cref="GuiPage.MultipleInstancesAllowed"/> is true. </summary>
        static Dictionary<System.Type, GuiPage> openSingletonUIObjects = new();


        private static bool initialized = false;

    #region Construction

        /// <summary> Constructor you must call before interacting with this class. </summary>
        public static void Initialize(Node parent) {
            if (initialized)
                return;

            initialized = true;

            node = new Node() { Name = nameof(Gui) };
            parent.CallDeferred(Node.MethodName.AddChild, node);

            packedGUIObjects = LoadUiObjects();
        }

        /// <summary> Loads and instantiates one instance of each <see cref="GuiPage"/> from hard the drive. </summary>
        private static Dictionary<Type, PackedScene> LoadUiObjects() {

            Type[] uiObjectTypes = Assembly.GetCallingAssembly()
                                           .GetTypes()
                                           .Where(type => type.IsSubclassOf(typeof(GuiPage)) && type.IsAbstract is false)
                                           .ToArray();

            var uiElements = new Dictionary<Type, PackedScene>(uiObjectTypes.Length);

            foreach (Type uiObjectType in uiObjectTypes) {
            #if LogLoadingMessages
                Godot.GD.Print($"Loading {nameof(GuiPage)} {uiObjectType.Name}");
            #endif

                if (TryGetScenePathFor(uiObjectType, out string scenePath) is false) {
                    Godot.GD.PrintErr($"{uiObjectType.Name} doesn't have the {nameof(ScriptPathAttribute)} applied to it, we can't determine where it's scene file is");
                    continue;
                }

                var packedScene = ResourceLoader.Load<PackedScene>(scenePath);
                if (packedScene is null) {
                    Godot.GD.PrintErr($"cannot load scene for {uiObjectType.Name}: there is no scene file to load at '{scenePath}', it's expected that your scene and script file are located in the same directory with the same name");
                    continue;
                }

                try {
                    var uiObject = packedScene.Instantiate() as GuiPage;
                    if (uiObject is null) {
                        Godot.GD.PrintErr($"the scene file at '{scenePath}' should contain a top level node inheriting from {nameof(GuiPage)}, skipping loading of {uiObjectType.Name}");
                        continue;
                    }

                    uiElements.Add(uiObject.GetType(), packedScene);
                }
                catch {
                    Godot.GD.PrintErr($"failed to instantiate scene file at '{scenePath}' for {uiObjectType.Name}");
                }
            }

            return uiElements;
        }

    #endregion Construction

        /// <summary> Special constructor for <see cref="GuiPage"/> of any type. </summary>
        public static T? Open<T>() where T : GuiPage {
            var packedScene = packedGUIObjects[typeof(T)];
            var uiObject = packedScene.Instantiate() as T;
            if (uiObject is null) {
                // log error object is not listed type?
                return null;
            }

            Register(uiObject);

            // open method on the page is called when the object enters the tree in the GUIPage base class
            node.AddChildDeferred(uiObject);
            return uiObject;
        }

        /// <summary>
        /// Closes a <see cref="GuiPage"/> without needing a direct reference to it. <br/>
        /// Use <see cref="GuiPage.Close"/> in other cases.
        /// </summary>
        public static void Close<T>() where T : GuiPage {
            //#if DEBUG
            if (openSingletonUIObjects.TryGetValue(typeof(T), out GuiPage? uiObject) is false) {
                // log error, ui object of type is either not open, or not a singleton type, try calling instance method or ensure object is open
                return;
            }
            //#endif

            uiObject.Close();
        }

        /// <summary> Gets a reference to a <see cref="GuiPage"/> of Type T. </summary>
        public static T Get<T>() where T : GuiPage {
        #if DEBUG
            if (openSingletonUIObjects.TryGetValue(typeof(T), out GuiPage? uiObject) is false) {
                // log error, ui object of type is either not open, or not a singleton type, try calling instance method or ensure object is open
                return null!;
            }
        #endif

            return (openSingletonUIObjects[typeof(T)] as T)!;
        }


    #region Private Methods

        /// <summary> Begins tracking a UI Object. </summary>
        static void Register<T>(T uiObject) where T : GuiPage {
            uiObject.CloseFinished += () => Unregister(uiObject);

            if (openSingletonUIObjects.ContainsKey(typeof(T))) {
                Godot.GD.Print($"prevented second instance of {typeof(T).Name} from opening");
                return;
            }

            openSingletonUIObjects.Add(typeof(T), uiObject);
        }

        /// <summary> Ends tracking a <see cref="GuiPage"/> and destroys the object. </summary>
        static void Unregister<T>(T uiObject) where T : GuiPage {

            if (openSingletonUIObjects.ContainsKey(typeof(T)) is false) {
                Godot.GD.Print($"cannot unregister {typeof(T).Name} because it isn't registered");
                return;
            }

            openSingletonUIObjects.Remove(typeof(T));
            uiObject.QueueFree();
        }

        /// <remarks> All scripts we make inheriting from node should have the <see cref="ScriptPathAttribute"/> applied automatically. </remarks>
        static bool TryGetScenePathFor(Type guiObjectType, out string scenePath) {
            // we only want to fetch the script path attribute from the final derived type
            var pathAttribute = (ScriptPathAttribute?)Attribute.GetCustomAttribute(guiObjectType, typeof(ScriptPathAttribute), inherit: false);
            string? scriptPath = pathAttribute?.Path;
            if (scriptPath is null) {
                scenePath = string.Empty;
                return false;
            }
            
            int sceneNamingConvention = (int)Godot.ProjectSettings.GetSetting("editor/naming/scene_name_casing");
            switch (sceneNamingConvention) {
                default:
                case 0: // Auto
                case 1: // PascalCase
                    scenePath = scriptPath.ToPascalCase();
                    break;
                case 2: // snake_case
                    scriptPath = scriptPath.ToSnakeCase();
                    break;
            }

            // trim off the .cs file extension and replace it with .tscn
            scenePath = $"{scriptPath.Substring(0, scriptPath.Length - 3)}.tscn";
            return true;
        }

    #endregion Private Methods
    }

#nullable restore