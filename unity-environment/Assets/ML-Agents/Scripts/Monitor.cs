using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MLAgents
{
    /// <summary>
    /// Monitor is used to display information about the Agent within the Unity
    /// scene. Use the log function to add information to your monitor.
    /// </summary>
    public class Monitor : MonoBehaviour
    {
        /// <summary>
        /// The type of monitor the information must be displayed in.
        /// <slider> corresponds to a single rectangle whose width is given</slider>
        /// by a float between -1 and 1. (green is positive, red is negative)
        /// <hist> corresponds to n vertical sliders.</hist>
        /// <text> is a text field.</text>
        /// <bar> is a rectangle of fixed length to represent the proportions
        /// of a list of floats.</bar>
        /// </summary>
        public enum DisplayType
        {
            INDEPENDENT = 0,
            PROPORTION = 1
        }

        /// <summary>
        /// Represents how high above the target the monitors will be.
        /// </summary>
        [HideInInspector] static public float VerticalOffset = 3f;

        static bool s_isInstantiated;
        static GameObject s_canvas;
        static Dictionary<Transform, Dictionary<string, DisplayValue>> s_displayTransformValues;
        static Color[] s_barColors;

        struct DisplayValue
        {
            public float Time;
            public string StringValue;
            public float FloatValue;
            public float[] FloatArrayValues;

            public enum ValueTypes
            {
                FLOAT = 0,
                FLOATARRAY_INDEPENDENT = 1,
                FLOATARRAY_PROPORTION = 2,
                STRING = 3
            }

            public ValueTypes valueTypes;
        }

        static GUIStyle s_keyStyle;
        static GUIStyle s_valueStyle;
        static GUIStyle s_greenStyle;
        static GUIStyle s_redStyle;
        static GUIStyle[] s_colorStyle;
        static bool s_initialized;

        /// <summary>
        /// Use the Monitor.Log static function to attach information to a transform.
        /// </summary>
        /// <returns>The log.</returns>
        /// <param name="key">The name of the information you wish to Log.</param>
        /// <param name="value">The string value you want to display.</param>
        /// <param name="target">The transform you want to attach the information to.
        /// </param>
        public static void Log(string key, string value, Transform target = null)
        {
            if(!s_isInstantiated)
            {
                InstantiateCanvas();
                s_isInstantiated = true;
            }

            if(target == null)
            {
                target = s_canvas.transform;
            }

            if(!s_displayTransformValues.Keys.Contains(target))
            {
                s_displayTransformValues[target] = new Dictionary<string, DisplayValue>();
            }

            var displayValues = s_displayTransformValues[target];

            if(value == null)
            {
                RemoveValue(target, key);
                return;
            }

            if(!displayValues.ContainsKey(key))
            {
                var dv = new DisplayValue();
                dv.Time = Time.timeSinceLevelLoad;
                dv.StringValue = value;
                dv.valueTypes = DisplayValue.ValueTypes.STRING;
                displayValues[key] = dv;
                while (displayValues.Count > 20)
                {
                    var max = (displayValues.Aggregate((l, r) => l.Value.Time < r.Value.Time ? l : r).Key);
                    RemoveValue(target, max);
                }
            }
            else
            {
                var dv = displayValues[key];
                dv.StringValue = value;
                dv.valueTypes = DisplayValue.ValueTypes.STRING;
                displayValues[key] = dv;
            }
        }

        /// <summary>
        /// Use the Monitor.Log static function to attach information to a transform.
        /// </summary>
        /// <returns>The log.</returns>
        /// <param name="key">The name of the information you wish to Log.</param>
        /// <param name="value">The float value you want to display.</param>
        /// <param name="target">The transform you want to attach the information to.
        /// </param>
        public static void Log(string key, float value, Transform target = null)
        {
            if(!s_isInstantiated)
            {
                InstantiateCanvas();
                s_isInstantiated = true;
            }

            if(target == null)
            {
                target = s_canvas.transform;
            }

            if(!s_displayTransformValues.Keys.Contains(target))
            {
                s_displayTransformValues[target] = new Dictionary<string, DisplayValue>();
            }

            var displayValues = s_displayTransformValues[target];

            if(!displayValues.ContainsKey(key))
            {
                var dv = new DisplayValue();
                dv.Time = Time.timeSinceLevelLoad;
                dv.FloatValue = value;
                dv.valueTypes = DisplayValue.ValueTypes.FLOAT;
                displayValues[key] = dv;
                while (displayValues.Count > 20)
                {
                    var max = (displayValues.Aggregate((l, r) => l.Value.Time < r.Value.Time ? l : r).Key);
                    RemoveValue(target, max);
                }
            }
            else
            {
                var dv = displayValues[key];
                dv.FloatValue = value;
                dv.valueTypes = DisplayValue.ValueTypes.FLOAT;
                displayValues[key] = dv;
            }
        }

        /// <summary>
        /// Use the Monitor.Log static function to attach information to a transform.
        /// </summary>
        /// <returns>The log.</returns>
        /// <param name="key">The name of the information you wish to Log.</param>
        /// <param name="value">The array of float you want to display.</param>
        /// <param name="displayType">The type of display.</param>
        /// <param name="target">The transform you want to attach the information to.
        /// </param>
        public static void Log(string key, float[] value, Transform target = null,
            DisplayType displayType = DisplayType.INDEPENDENT)
        {
            if(!s_isInstantiated)
            {
                InstantiateCanvas();
                s_isInstantiated = true;
            }

            if(target == null)
            {
                target = s_canvas.transform;
            }

            if(!s_displayTransformValues.Keys.Contains(target))
            {
                s_displayTransformValues[target] = new Dictionary<string, DisplayValue>();
            }

            var displayValues = s_displayTransformValues[target];

            if(!displayValues.ContainsKey(key))
            {
                var dv = new DisplayValue();
                dv.Time = Time.timeSinceLevelLoad;
                dv.FloatArrayValues = value;
                if(displayType == DisplayType.INDEPENDENT)
                {
                    dv.valueTypes = DisplayValue.ValueTypes.FLOATARRAY_INDEPENDENT;
                }
                else
                {
                    dv.valueTypes = DisplayValue.ValueTypes.FLOATARRAY_PROPORTION;
                }

                displayValues[key] = dv;
                while (displayValues.Count > 20)
                {
                    var max = (displayValues.Aggregate((l, r) => l.Value.Time < r.Value.Time ? l : r).Key);
                    RemoveValue(target, max);
                }
            }
            else
            {
                var dv = displayValues[key];
                dv.FloatArrayValues = value;
                if(displayType == DisplayType.INDEPENDENT)
                {
                    dv.valueTypes = DisplayValue.ValueTypes.FLOATARRAY_INDEPENDENT;
                }
                else
                {
                    dv.valueTypes = DisplayValue.ValueTypes.FLOATARRAY_PROPORTION;
                }

                displayValues[key] = dv;
            }
        }


        /// <summary>
        /// Remove a value from a monitor.
        /// </summary>
        /// <param name="target">
        /// The transform to which the information is attached.
        /// </param>
        /// <param name="key">The key of the information you want to remove.</param>
        private static void RemoveValue(Transform target, string key)
        {
            if(target == null)
            {
                target = s_canvas.transform;
            }

            if(s_displayTransformValues.Keys.Contains(target))
            {
                if(s_displayTransformValues[target].ContainsKey(key))
                {
                    s_displayTransformValues[target].Remove(key);
                    if(s_displayTransformValues[target].Keys.Count == 0)
                    {
                        s_displayTransformValues.Remove(target);
                    }
                }
            }

        }

        /// <summary>
        /// Remove all information from a monitor.
        /// </summary>
        /// <param name="target">
        /// The transform to which the information is attached.
        /// </param>
        public static void RemoveAllValues(Transform target)
        {
            if(target == null)
            {
                target = s_canvas.transform;
            }

            if(s_displayTransformValues.Keys.Contains(target))
            {
                s_displayTransformValues.Remove(target);
            }

        }

        /// <summary>
        /// Use SetActive to enable or disable the Monitor via script
        /// </summary>
        /// <param name="active">Value to set the Monitor's status to.</param>
        public static void SetActive(bool active)
        {
            if(!s_isInstantiated)
            {
                InstantiateCanvas();
                s_isInstantiated = true;

            }

            if(s_canvas != null)
            {
                s_canvas.SetActive(active);
            }

        }

        /// Initializes the canvas.
        static void InstantiateCanvas()
        {
            s_canvas = GameObject.Find("AgentMonitorCanvas");
            if(s_canvas == null)
            {
                s_canvas = new GameObject();
                s_canvas.name = "AgentMonitorCanvas";
                s_canvas.AddComponent<Monitor>();
            }

            s_displayTransformValues = new Dictionary<Transform, Dictionary<string, DisplayValue>>();
        }

        /// <summary> <inheritdoc/> </summary>
        void OnGUI()
        {
            if(!s_initialized)
            {
                Initialize();
                s_initialized = true;
            }

            var toIterate = s_displayTransformValues.Keys.ToList();
            foreach (var target in toIterate)
            {
                if(target == null)
                {
                    s_displayTransformValues.Remove(target);
                    continue;
                }

                var widthScaler = (Screen.width / 1000f);
                var keyPixelWidth = 100 * widthScaler;
                var keyPixelHeight = 20 * widthScaler;
                var paddingwidth = 10 * widthScaler;

                var scale = 1f;
                var origin = new Vector3(Screen.width / 2 - keyPixelWidth, Screen.height);
                if(!(target == s_canvas.transform))
                {
                    var cam2Obj = target.position - Camera.main.transform.position;
                    scale = Mathf.Min(1, 20f / (Vector3.Dot(cam2Obj, Camera.main.transform.forward)));
                    var worldPosition =
                        Camera.main.WorldToScreenPoint(target.position + new Vector3(0, VerticalOffset, 0));
                    origin = new Vector3(worldPosition.x - keyPixelWidth * scale, Screen.height - worldPosition.y);
                }

                keyPixelWidth *= scale;
                keyPixelHeight *= scale;
                paddingwidth *= scale;
                s_keyStyle.fontSize = (int) (keyPixelHeight * 0.8f);
                if(s_keyStyle.fontSize < 2)
                {
                    continue;
                }


                var displayValues = s_displayTransformValues[target];

                var index = 0;
                var orderedKeys = displayValues.Keys.OrderBy(x => -displayValues[x].Time);
                float[] vals;
                GUIStyle s;
                foreach (var key in orderedKeys)
                {
                    s_keyStyle.alignment = TextAnchor.MiddleRight;
                    GUI.Label(
                        new Rect(origin.x, origin.y - (index + 1) * keyPixelHeight, keyPixelWidth, keyPixelHeight), key,
                        s_keyStyle);
                    switch (displayValues[key].valueTypes)
                    {
                        case DisplayValue.ValueTypes.STRING:
                            s_valueStyle.alignment = TextAnchor.MiddleLeft;
                            GUI.Label(
                                new Rect(origin.x + paddingwidth + keyPixelWidth,
                                    origin.y - (index + 1) * keyPixelHeight, keyPixelWidth, keyPixelHeight),
                                displayValues[key].StringValue, s_valueStyle);
                            break;
                        case DisplayValue.ValueTypes.FLOAT:
                            var sliderValue = displayValues[key].FloatValue;
                            sliderValue = Mathf.Min(1f, sliderValue);
                            s = s_greenStyle;
                            if(sliderValue < 0)
                            {
                                sliderValue = Mathf.Min(1f, -sliderValue);
                                s = s_redStyle;
                            }

                            GUI.Box(
                                new Rect(origin.x + paddingwidth + keyPixelWidth,
                                    origin.y - (index + 0.9f) * keyPixelHeight, keyPixelWidth * sliderValue,
                                    keyPixelHeight * 0.8f), GUIContent.none, s);
                            break;

                        case DisplayValue.ValueTypes.FLOATARRAY_INDEPENDENT:
                            var histWidth = 0.15f;
                            vals = displayValues[key].FloatArrayValues;
                            for (var i = 0; i < vals.Length; i++)
                            {
                                var value = Mathf.Min(vals[i], 1);
                                s = s_greenStyle;
                                if(value < 0)
                                {
                                    value = Mathf.Min(1f, -value);
                                    s = s_redStyle;
                                }

                                GUI.Box(
                                    new Rect(
                                        origin.x + paddingwidth + keyPixelWidth +
                                        (keyPixelWidth * histWidth + paddingwidth / 2) * i,
                                        origin.y - (index + 0.1f) * keyPixelHeight, keyPixelWidth * histWidth,
                                        -keyPixelHeight * value), GUIContent.none, s);
                            }

                            break;

                        case DisplayValue.ValueTypes.FLOATARRAY_PROPORTION:
                            var valsSum = 0f;
                            var valsCum = 0f;
                            vals = displayValues[key].FloatArrayValues;
                            foreach (var f in vals)
                            {
                                valsSum += Mathf.Max(f, 0);
                            }

                            if(valsSum < float.Epsilon)
                            {
                                Debug.LogError(string.Format(
                                    "The Monitor value for key {0} " + "must be a list or array of " +
                                    "positive values and cannot " + "be empty.", key));
                            }
                            else
                            {
                                for (var i = 0; i < vals.Length; i++)
                                {
                                    var value = Mathf.Max(vals[i], 0) / valsSum;
                                    GUI.Box(
                                        new Rect(origin.x + paddingwidth + keyPixelWidth + keyPixelWidth * valsCum,
                                            origin.y - (index + 0.9f) * keyPixelHeight, keyPixelWidth * value,
                                            keyPixelHeight * 0.8f), GUIContent.none,
                                        s_colorStyle[i % s_colorStyle.Length]);
                                    valsCum += value;

                                }

                            }

                            break;
                    }

                    index++;
                }
            }
        }

        /// Helper method used to initialize the GUI. Called once.
        void Initialize()
        {
            s_keyStyle = GUI.skin.label;
            s_valueStyle = GUI.skin.label;
            s_valueStyle.clipping = TextClipping.Overflow;
            s_valueStyle.wordWrap = false;
            s_barColors = new Color[] {Color.magenta, Color.blue, Color.cyan, Color.green, Color.yellow, Color.red};
            s_colorStyle = new GUIStyle[s_barColors.Length];
            for (var i = 0; i < s_barColors.Length; i++)
            {
                var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                texture.SetPixel(0, 0, s_barColors[i]);
                texture.Apply();
                var staticRectStyle = new GUIStyle();
                staticRectStyle.normal.background = texture;
                s_colorStyle[i] = staticRectStyle;
            }

            s_greenStyle = s_colorStyle[3];
            s_redStyle = s_colorStyle[5];
        }
    }
}