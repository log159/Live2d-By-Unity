#define IS_SIMPLE
//#define IS_UNABRIDGED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Framework.Json;
using System;
using System.IO;
using Live2D.Cubism.Rendering;
using Live2D.Cubism;
using Live2D.Cubism.Framework;
using JetBrains.Annotations;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using Live2D.Cubism.Framework.LookAt;
using Unity.VisualScripting;
using Live2D.Cubism.Framework.Pose;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Framework.MotionFade;
using Live2D.Cubism.Framework.MouthMovement;
using UnityEngine.UIElements;
using Live2D.Cubism.Core;
using System.Data.SqlTypes;
using System.Drawing;
using System.Data.Common;
using System.Text.RegularExpressions;

public class Model : MonoBehaviour
{
    /// <summary>
    /// 成员属性
    /// </summary>

    //Json File
    private CubismModel3Json Live2dModel3Json = null;
    //Model Object
    private CubismModel Live2dCubismModel = null;
    //GameObject
    private GameObject Live2dObject = null;
    //Parameters Object
    private GameObject Live2dParameters = null;
    //Parts Object
    private GameObject Live2dParts = null;
    //Drawables Object
    private GameObject Live2dDrawables = null;
    //Parameters Object Transforms
    private Transform[] Live2dParametersTransforms = null;
    //Parts Object Transforms
    private Transform[] Live2dPartsTransforms = null;
    //Drawables Object Transforms
    private Transform[] Live2dDrawablesTransforms = null;
    //Param Items
    private Transform TransformParamAngleX = null;
    private Transform TransformParamAngleY = null;
    private Transform TransformParamAngleZ = null;
    private Transform TransformParamBodyAngleX = null;
    private Transform TransformParamBodyAngleY = null;
    private Transform TransformParamBodyAngleZ = null;
    private Transform TransformParamEyeBallX = null;
    private Transform TransformParamEyeBallY = null;
    //Open Models
    private static List<GameObject> OpeningLive2dModelList = new List<GameObject>();
    //Params Dictionary
    private static Dictionary<string, float> ParamItemsDic = new Dictionary<string, float>();
    //Draws Dictionary
    private static List<Tuple<string, float>> DrawItemsList = new List<Tuple<string, float>>();


    //public information
    //GameObject Transform
    public static Transform Live2dObjectTransform = null;

    void Start()
    {
        InitModel();
    }
    public void InitModel()
    {
        //删除上一个模型
        DestroyModel();
        //初始化模型
        //string path = "D:/XXX/XXX.model3.json";
        string path = "D:\\Model\\XXX.model3.json";
        InitModelByPath(path);

    }

    /// <summary>
    /// 通过Path初始化模型
    /// </summary>
    private void InitModelByPath(string path)
    {
        Config.ModelPath = path;
        string filePath = Config.ModelPath;
        int index = filePath.Length - 1;
        for (; index >= 0; --index)
            if (filePath[index] == (char)'\\' || filePath[index] == (char)'/')
                break;
        Config.DirectoryPath = filePath.Substring(0, index + 1);

        try
        {
#if IS_SIMPLE
            //通过Json初始化模型
            InitJsonFunction();
            //脚本及参数初始化
            InitModelFunction();
#elif IS_UNABRIDGED
            //通过Json初始化模型
            InitJsonFunction();
            //脚本及参数初始化
            InitModelFunction();
            //生成模型的谐波控件列表
            InitModelHarmonic();
            //根据谐波控件列表初始化控件参数
            InitSelfModelParameters();
            InitSelfModelDrawables();
#endif
            //other...
            //指定控件隐藏
            DrawItemsList.Add(new Tuple<string, float>("XXX", 0));
            //指定控件偏移
            ParamItemsDic["XXX"] = 3f;
        }
        catch (Exception e)
        {
            Debug.Log("异常信息：" + e.Message);
        }
    }

    /// <summary>
    /// 删除模型
    /// </summary>
    public void DestroyModel()
    {
        Destroy(Live2dParameters);
        Destroy(Live2dParts);
        Destroy(Live2dDrawables);
        Destroy(Live2dCubismModel);

        if (Live2dParametersTransforms != null)
            foreach (Transform itemname in Live2dParametersTransforms)
                Destroy(itemname);

        if (Live2dPartsTransforms != null)
            foreach (Transform itemname in Live2dPartsTransforms)
                Destroy(itemname);

        if (Live2dDrawablesTransforms != null)
            foreach (Transform itemname in Live2dDrawablesTransforms)
                Destroy(itemname);

        Destroy(TransformParamAngleX);
        Destroy(TransformParamAngleY);
        Destroy(TransformParamAngleZ);
        Destroy(TransformParamBodyAngleX);
        Destroy(TransformParamBodyAngleY);
        Destroy(TransformParamBodyAngleZ);
        Destroy(TransformParamEyeBallX);
        Destroy(TransformParamEyeBallY);


        while (OpeningLive2dModelList.Count > 0)
        {
            GameObject lt = OpeningLive2dModelList[0];
            Destroy(lt);
            OpeningLive2dModelList.RemoveAt(0);
        }
        OpeningLive2dModelList.Clear();
        ParamItemsDic.Clear();

        Resources.UnloadUnusedAssets();

    }

    /// <summary>
    /// 通过Json初始化模型
    /// </summary>
    private void InitJsonFunction()
    {
        string fileName = Path.GetFileName(Config.ModelPath);

        //名字
        Config.PersonName = fileName.Substring(0, fileName.Length - ".model3.json".Length);

        //读取Json 文件
        this.Live2dModel3Json = CubismModel3Json.LoadAtPath(Config.ModelPath, BuiltInLoadAtPath);
        if (Live2dModel3Json != null)
            Live2dCubismModel = Live2dModel3Json.ToModel();
        else
        {
            Application.Quit();
            return;
        }
        //Live2D Object
        this.Live2dObject = GameObject.Find(Config.PersonName);
        this.Live2dObject.name = Config.ModelName + Config.ModelId.ToString();
        Config.ModelId++;

        OpeningLive2dModelList.Add(this.Live2dObject);
        if (Live2dObject == null)
            throw new Exception("Object is null");
        Live2dObjectTransform = Live2dObject.transform;
        //Parameters Object
        this.Live2dParameters = Live2dObject.transform.Find("Parameters").gameObject;
        if (Live2dParameters == null)
            Debug.Log("Parameters is null");
        else
            this.Live2dParametersTransforms = this.Live2dParameters.GetComponentsInChildren<Transform>();
        //Parts Object
        this.Live2dParts = Live2dObject.transform.Find("Parts").gameObject;
        if (Live2dParts == null)
            Debug.Log("Parts is null");
        else
            this.Live2dPartsTransforms = this.Live2dParts.GetComponentsInChildren<Transform>();
        //Drawables Object
        this.Live2dDrawables = Live2dObject.transform.Find("Drawables").gameObject;
        if (Live2dDrawables == null)
            Debug.Log("Drawables is null");
        else
            this.Live2dDrawablesTransforms = this.Live2dDrawables.GetComponentsInChildren<Transform>();
    }

    /// <summary>
    /// 生成模型控件列表
    /// </summary>
    private void InitModelHarmonic()
    {

        string directoryPath = Config.DirectoryPath;
        if (directoryPath == null) return;

        CubismModel cubismModel = Live2dObject.FindCubismModel();
        Vector3 vector3 = new Vector3();

        Dictionary<string, string> parameterDic = new();
        Dictionary<string, string> drawableDic = new();
        //控件偏移
        for (int i = 0; i < cubismModel.Parameters.Length; ++i)
        {
            CubismParameter cubismParameter = cubismModel.Parameters[i];

            vector3.x = cubismParameter.DefaultValue;
            vector3.y = cubismParameter.MinimumValue;
            vector3.z = cubismParameter.MaximumValue;
            parameterDic[cubismParameter.name] = ("\"" + vector3.x.ToString() + "," + vector3.y.ToString() + "," + vector3.z.ToString() + "\"");
        }
        //控件显示隐藏
        for (int i = 0; i < cubismModel.Drawables.Length; ++i)
        {
            CubismDrawable cubismDrawable = cubismModel.Drawables[i];
            drawableDic[cubismDrawable.name] = "\"1\"";
        }
        Dictionary<string, Dictionary<string, string>> data = new();
        data.Add(Config.BaseParameterName, parameterDic);
        data.Add(Config.BaseDrawable, drawableDic);
        string iniPath = directoryPath + Config.ModelConfigFileName;
        Debug.Log("ini path:" + iniPath);
        if (!FileOperate.FileExists(iniPath))
        {
            FileStream fs = File.Create(iniPath);
            fs.Close();
        }
        FileOperate.WriteIniFile(data, iniPath);
    }


    /// <summary>
    /// 初始化模型脚本以及参数信息
    /// </summary>
    private void InitModelFunction()
    {
        if (Live2dObject == null) { return; }
        //设置默认挂载脚本
        AddComponentFunction();
        //设置模型大小
        SetModelSize(new Vector3(Config.ScaleProportionItem.Param, Config.ScaleProportionItem.Param, 1));
        //设置图层正确显示
        Live2dObject.GetComponent<CubismRenderController>().SortingMode = CubismSortingMode.BackToFrontOrder;
        //Live2dObject.GetComponent<CubismRenderController>().SortingOrder = (Config.ModelId - 1) * 500;//多个Model设置渲染优先级

        //控件视线跟踪固定参数
        Live2dObject.GetComponent<CubismLookController>().BlendMode = CubismParameterBlendMode.Override;
        Live2dObject.GetComponent<CubismEyeBlinkController>().BlendMode = CubismParameterBlendMode.Override;
        Live2dObject.GetComponent<CubismMouthController>().BlendMode = CubismParameterBlendMode.Override;
        Live2dObject.GetComponent<CubismAudioMouthInput>().SamplingQuality = CubismAudioSamplingQuality.Maximum;
        // 使用这个方法  
        FindTransform("PAX", out TransformParamAngleX);
        FindTransform("PAY", out TransformParamAngleY);
        FindTransform("PAZ", out TransformParamAngleZ);
        FindTransform("PBAX", out TransformParamBodyAngleX);
        FindTransform("PBAY", out TransformParamBodyAngleY);
        FindTransform("PBAZ", out TransformParamBodyAngleZ);
        FindTransform("PEBX", out TransformParamEyeBallX);
        FindTransform("PEBY", out TransformParamEyeBallY);

        if (TransformParamAngleX != null) TransformParamAngleX.gameObject.GetComponent<CubismLookParameter>().Axis = CubismLookAxis.X;
        if (TransformParamAngleY != null) TransformParamAngleY.gameObject.GetComponent<CubismLookParameter>().Axis = CubismLookAxis.Y;
        if (TransformParamAngleZ != null) TransformParamAngleZ.gameObject.GetComponent<CubismLookParameter>().Axis = CubismLookAxis.Z;

        if (TransformParamBodyAngleX != null) TransformParamBodyAngleX.gameObject.GetComponent<CubismLookParameter>().Axis = CubismLookAxis.X;
        if (TransformParamBodyAngleY != null) TransformParamBodyAngleY.gameObject.GetComponent<CubismLookParameter>().Axis = CubismLookAxis.Y;
        if (TransformParamBodyAngleZ != null) TransformParamBodyAngleZ.gameObject.GetComponent<CubismLookParameter>().Axis = CubismLookAxis.Z;

        if (TransformParamEyeBallX != null) TransformParamEyeBallX.gameObject.GetComponent<CubismLookParameter>().Axis = CubismLookAxis.X;
        if (TransformParamEyeBallY != null) TransformParamEyeBallY.gameObject.GetComponent<CubismLookParameter>().Axis = CubismLookAxis.Y;

        //变化时更新模型参数
        UpdateModelCondition();
    }


    // 设置模型大小
    private void SetModelSize(Vector3 vector3)
    {
        Live2dObject.transform.localScale = vector3;
    }
    // 设置模型位置
    private void SetModelTransformPosition(Vector3 vector3)
    {
        Live2dObject.transform.position = vector3;
    }
    //设置模型旋转
    private void SetModelTransformRotation(Vector3 vector3)
    {
        Live2dObject.transform.rotation = Quaternion.Euler(vector3.x, vector3.y, vector3.z);
    }
    /// <summary>
    /// 初始模型控件协调
    /// </summary>
    public void InitModelParameters()
    {
        CubismModel cubismModel = Live2dObject.FindCubismModel();
        for (int i = 0; i < cubismModel.Parameters.Length; ++i)
        {
            CubismParameter cubismParameter = cubismModel.Parameters[i];
            cubismParameter.Value = cubismParameter.DefaultValue;
            AddParameterList(cubismParameter.Id, cubismParameter.DefaultValue);

        }
    }
    /// <summary>
    /// 初始模型控件显示
    /// </summary>
    public void InitModelDrawables()
    {
        foreach (Transform itemname in Live2dDrawablesTransforms)
        {
            AddDrawableList(itemname.gameObject.name, 1);
        }
    }

    /// <summary>
    /// 自定义控件协调更新
    /// </summary>
    public void InitSelfModelParameters()
    {
        string filePath = Config.DirectoryPath + Config.ModelConfigFileName;
        Dictionary<string, Dictionary<string, string>> dataDic = FileOperate.ParseIniFile(filePath);
        if (!dataDic.ContainsKey(Config.BaseParameterChangeName)) return;
        Dictionary<string, string> parameterDic = dataDic[Config.BaseParameterChangeName];
        foreach (KeyValuePair<string, string> item in parameterDic)
            AddParameterList(item.Key, float.Parse(item.Value) / 100f);
    }

    /// <summary>
    /// 自定义控件渲染更新
    /// </summary>
    public void InitSelfModelDrawables()
    {
        string filePath = Config.DirectoryPath + Config.ModelConfigFileName;
        Dictionary<string, Dictionary<string, string>> dataDic = FileOperate.ParseIniFile(filePath);
        if (!dataDic.ContainsKey(Config.BaseDrawableChange)) return;
        Dictionary<string, string> drawableDic = dataDic[Config.BaseDrawableChange];

        foreach (KeyValuePair<string, string> item in drawableDic)
            AddDrawableList(item.Key, float.Parse(item.Value));
    }

    /// <summary>
    /// 更新模型参数
    /// </summary>
    public void UpdateModelCondition()
    {
        if (Live2dObject == null) { return; }
        //更新模型大小
        SetModelSize(new Vector3(Config.ScaleProportionItem.Param, Config.ScaleProportionItem.Param, 1));
        //更新模型位置
        SetModelTransformPosition(new Vector3(Config.PositionXItem.Param, Config.PositionYItem.Param, 0));
        //更新模型旋转
        SetModelTransformRotation(new Vector3(Config.RotationRXItem.Param, Config.RotationRYItem.Param, Config.RotationRZItem.Param));

        gameObject.transform.Find("Target").GetComponent<LookMouse>().SetLookMouse(Config.IsLookMouse);

        //更新视线追踪参数
        if (TransformParamAngleX != null) TransformParamAngleX.gameObject.GetComponent<CubismLookParameter>().Factor = Config.ParamAngleItem.Param;
        if (TransformParamAngleY != null) TransformParamAngleY.gameObject.GetComponent<CubismLookParameter>().Factor = Config.ParamAngleItem.Param;
        if (TransformParamAngleZ != null) TransformParamAngleZ.gameObject.GetComponent<CubismLookParameter>().Factor = Config.ParamAngleItem.Param;

        if (TransformParamBodyAngleX != null) TransformParamBodyAngleX.gameObject.GetComponent<CubismLookParameter>().Factor = Config.ParamBodyAngleItem.Param;
        if (TransformParamBodyAngleY != null) TransformParamBodyAngleY.gameObject.GetComponent<CubismLookParameter>().Factor = Config.ParamBodyAngleItem.Param;
        if (TransformParamBodyAngleZ != null) TransformParamBodyAngleZ.gameObject.GetComponent<CubismLookParameter>().Factor = Config.ParamBodyAngleItem.Param;

        if (TransformParamEyeBallX != null) TransformParamEyeBallX.gameObject.GetComponent<CubismLookParameter>().Factor = Config.ParamEyeBallItem.Param;
        if (TransformParamEyeBallY != null) TransformParamEyeBallY.gameObject.GetComponent<CubismLookParameter>().Factor = Config.ParamEyeBallItem.Param;

        //设置人物看向鼠标速度
        if (Live2dObject.GetComponent<CubismLookController>() != null) Live2dObject.GetComponent<CubismLookController>().Damping = Config.DampingItem.Param;
        if (Live2dObject.GetComponent<CubismLookController>() != null) Live2dObject.GetComponent<CubismLookController>().Center = gameObject.transform;

        //设置眨眼参数
        if (Live2dObject.GetComponent<CubismAutoEyeBlinkInput>() != null) Live2dObject.GetComponent<CubismAutoEyeBlinkInput>().Mean = Config.MeanItem.Param;
        if (Live2dObject.GetComponent<CubismAutoEyeBlinkInput>() != null) Live2dObject.GetComponent<CubismAutoEyeBlinkInput>().MaximumDeviation = Config.MaximumDeviationItem.Param;
        if (Live2dObject.GetComponent<CubismAutoEyeBlinkInput>() != null) Live2dObject.GetComponent<CubismAutoEyeBlinkInput>().Timescale = Config.TimescaleItem.Param;

        //设置张嘴参数
        if (Live2dObject.GetComponent<CubismMouthController>() != null) Live2dObject.GetComponent<CubismMouthController>().MouthOpening = Config.MouthOpeningItem.Param;
        //设置音频参数
        if (Live2dObject.GetComponent<CubismAudioMouthInput>() != null) Live2dObject.GetComponent<CubismAudioMouthInput>().Gain = Config.GainItem.Param;
        if (Live2dObject.GetComponent<CubismAudioMouthInput>() != null) Live2dObject.GetComponent<CubismAudioMouthInput>().Smoothing = Config.SmoothingItem.Param;
    }

    /// <summary>
    /// Parameters控件协调
    /// </summary>
    public void AddParameterList(string itemName, float value)
    {
        ParamItemsDic[itemName] = value;
    }
    /// <summary>
    /// Drawables控件协调
    /// </summary>
    public void AddDrawableList(string itemName, float value)
    {
        DrawItemsList.Add(Tuple.Create(itemName, value));
    }
    private void LateUpdate()
    {
        CubismModel cubismModel = Live2dObject.FindCubismModel();
        if (cubismModel == null)
        {
            return;
        }
        //控件偏移 & 谐波 要求实时刷新
        foreach (KeyValuePair<string, float> paramPair in ParamItemsDic)
        {
            CubismParameter parameterItem = cubismModel.Parameters.FindById(paramPair.Key);
            if (parameterItem != null)
                parameterItem.BlendToValue(CubismParameterBlendMode.Override, paramPair.Value);
        }

        //控件显示隐藏执行一次就可以
        if (DrawItemsList.Count > 0)
        {
            Tuple<string, float> drawTuple = DrawItemsList[0];
            if (drawTuple != null)
            {
                CubismDrawable drawableItem = cubismModel.Drawables.FindById(drawTuple.Item1);
                if (drawableItem != null)
                {
                    Debug.Log("隐藏或显示变化的控件: " + drawTuple.Item1 + "  " + drawTuple.Item2);
                    drawableItem.gameObject.SetActive(drawTuple.Item2 > 0.0 ? true : false);
                }
            }
            DrawItemsList.RemoveAt(0);
        }
    }

    /// <summary>
    /// 默认挂载脚本或物体
    /// </summary>
    private void AddComponentFunction()
    {
        //挂载默认的脚本
        if (Live2dObject.GetComponent<CubismUpdateController>() == null) Live2dObject.AddComponent<CubismUpdateController>();
        if (Live2dObject.GetComponent<CubismParameterStore>() == null) Live2dObject.AddComponent<CubismParameterStore>();
        if (Live2dObject.GetComponent<CubismPoseController>() == null) Live2dObject.AddComponent<CubismPoseController>();
        if (Live2dObject.GetComponent<CubismExpressionController>() == null) Live2dObject.AddComponent<CubismExpressionController>();
        if (Live2dObject.GetComponent<CubismFadeController>() == null) Live2dObject.AddComponent<CubismFadeController>();
        if (Live2dObject.GetComponent<CubismAutoEyeBlinkInput>() == null) Live2dObject.AddComponent<CubismAutoEyeBlinkInput>();
        if (Live2dObject.GetComponent<CubismMouthController>() == null) Live2dObject.AddComponent<CubismMouthController>();
        if (Live2dObject.GetComponent<CubismAudioMouthInput>() == null) Live2dObject.AddComponent<CubismAudioMouthInput>();
        if (Live2dObject.GetComponent<CubismLookController>() == null) Live2dObject.AddComponent<CubismLookController>();
        if (Live2dObject.GetComponent<CubismEyeBlinkController>() == null) Live2dObject.AddComponent<CubismEyeBlinkController>();


        foreach (string itemname in Config.NeedAddCubismLookParameterObjectStrings)
        {
            Transform transformItem = Live2dParameters.transform.Find(itemname);
            if (transformItem != null)
            {
                if (transformItem.gameObject.GetComponent<CubismLookParameter>() == null)
                    transformItem.gameObject.AddComponent<CubismLookParameter>();
            }
        }
        //张嘴
        foreach (string item in Config.NeedAddCubismMouthParameterObjectStrings)
        {
            Transform transformParamMouthOpenY = Live2dParameters.transform.Find(item);
            if (transformParamMouthOpenY != null)
            {
                if (transformParamMouthOpenY.gameObject.GetComponent<CubismMouthParameter>() == null)
                    transformParamMouthOpenY.gameObject.AddComponent<CubismMouthParameter>();
            }
        }
        //眨眼
        foreach (string item in Config.NeedAddCubismEyeBlinkParameterObjectStrings)
        {
            Transform TransformParamEyeOpen = Live2dParameters.transform.Find(item);
            if (TransformParamEyeOpen != null)
            {
                if (TransformParamEyeOpen.gameObject.GetComponent<CubismEyeBlinkParameter>() == null)
                    TransformParamEyeOpen.gameObject.AddComponent<CubismEyeBlinkParameter>();
            }
        }

        //默认挂载的控件
        Transform transformTarget = gameObject.transform.Find("Target");
        if (transformTarget != null)
        {
            Live2dObject.GetComponent<CubismLookController>().Target = transformTarget.gameObject;
        }
        Transform transformAudioInput = gameObject.transform.Find("Audio");
        if (transformAudioInput != null)
        {
            Live2dObject.GetComponent<CubismAudioMouthInput>().AudioInput = transformAudioInput.gameObject.GetComponent<AudioSource>();
        }
    }
    /// <summary>
    /// 工具函数获取需要鼠标跟踪的控件
    /// </summary>
    void FindTransform(string key, out Transform result)
    {
        result = null;
        if (Config.keyValueParameters.TryGetValue(key, out var parameters))
        {
            foreach (var item in parameters)
            {
                result = Live2dParameters.transform.Find(item);
                if (result != null)
                    break;
            }
        }
    }
    /// <summary>
    /// 通过json加载模型信息
    /// </summary>
    private static object BuiltInLoadAtPath(Type assetType, string absolutePath)
    {
        if (assetType == typeof(byte[]))
        {
            return File.ReadAllBytes(absolutePath);
        }
        else if (assetType == typeof(string))
        {
            return File.ReadAllText(absolutePath);
        }
        else if (assetType == typeof(Texture2D))
        {
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.LoadImage(File.ReadAllBytes(absolutePath));
            return texture2D;
        }
        throw new NotSupportedException();
    }
}
