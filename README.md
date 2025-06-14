# UnityCommonSolution-TextureAsyncLoader

![GitHub](https://img.shields.io/badge/Unity-2021.3%2B-blue)
![GitHub](https://img.shields.io/badge/license-MIT-green)
![GitHub](https://img.shields.io/badge/Platform-Windows-red)

## 说明

本仓库是基于 [UnityAsyncImageLoader v0.1.2](https://github.com/Looooong/UnityAsyncImageLoader) 在`Unity2021.3`
版本开发，且原始代码已经包含在仓库中。
本包包含以下主要功能：

1. 图像加载、图像解码和Mipmap生成的工作转移到其他线程中，它可以使游戏运行更加流畅，并减少在加载大图像时Unity主线程的卡顿。

2. 将加载后的纹理数据进行加工，符合POT纹理的尺寸要求。
3. 运行时更改纹理尺寸，可在运行时压缩为ASTC和DXT5格式，以节省内存空间。
4. 创建`Texture2DInfo`类包装纹理数据。
5. 增加`Texture2D`类相关的实用拓展方法。

## 依赖

Unity注册包请在UPM窗口中选择`Unity Registry`项搜索安装；托管包请在UPM中使用`Add git RUL`方式进行安装：

+ Unity Burst
+ Unity Mathematics
+ UniTask：https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask

## 安装

1. **通过克隆仓库安装**

   将本仓库克隆到您的 Unity 项目的 `Assets` 目录下， 如果使用克隆方式安装，需要<span style="color: #00ff00;">
   手动添加上方的依赖项</span>。

   ```bash
   git clone https://github.com/Pixelsmao/UnityCommonSolution-NativeTextureAsyncLoader.git
   ```


2. **使用UPM进行安装：**

   在 Unity 编辑器中，点击顶部菜单栏,打开 Package Manager 窗口.

       Window > Package Manager

   在 Package Manager 窗口的左上角，点击 **+** 按钮，然后选择 **Add package from git URL...**。
   在弹出的输入框中，粘贴本仓库的 Git URL：

       https://github.com/Pixelsmao/UnityCommonSolution-NativeTextureAsyncLoader.git

然后点击 **Add**。

## 依赖库 UnityAsyncImageLoader 说明

在运行时加载大尺寸图像（大于2K）时，[`ImageConversion.LoadImage`](https://docs.unity3d.com/ScriptReference/ImageConversion.LoadImage.html)
和 `Texture2D.LoadImage` 会变得非常缓慢。它们在加载图像时会阻塞Unity的主线程，持续时间可能从几百毫秒到几秒不等。这对于那些希望在运行时动态加载图像的游戏和应用程序来说，是一个致命的问题。

本包旨在将图像加载、图像解码和Mipmap生成的工作转移到其他线程中。它可以使游戏运行更加流畅，并减少在加载大图像时Unity主线程的卡顿。

本包使用了 [FreeImage](https://freeimage.sourceforge.io/)，这是Unity用于处理图像数据的同一库。

### 加载器设置

```cs
  /// <summary>图像加载器使用的设置。</summary>
  public struct LoaderSettings {
    /// <summary>创建线性纹理。仅适用于创建新 <c>Texture2D</c> 的方法。默认为 false。</summary>
    public bool linear;
    /// <summary>加载后纹理数据在CPU上不可读。默认为 false。</summary>
    public bool markNonReadable;
    /// <summary>是否生成Mipmap。默认为 true。</summary>
    public bool generateMipmap;
    /// <summary>自动计算Mipmap层级数量。默认为 true。仅适用于创建新 <c>Texture2D</c> 的方法。</summary>
    public bool autoMipmapCount;
    /// <summary>Mipmap数量，包括基础层级。必须大于1。仅适用于创建新 <c>Texture2D</c> 的方法。</summary>
    public int mipmapCount;
    /// <summary>用于显式指定图像格式。默认为 FIF_UNKNOWN，图像格式将自动确定。</summary>
    public FreeImage.Format format;
    /// <summary>是否记录此方法捕获的异常。默认为 true。</summary>
    public bool logException;

    public static LoaderSettings Default => new LoaderSettings {
      linear = false,
      markNonReadable = false,
      generateMipmap = true,
      autoMipmapCount = true,
      format = FreeImage.Format.FIF_UNKNOWN,
      logException = true,
    };
  }
```

### 异步加载图像

```cs
  var imageData = File.ReadAllBytes();
  var texture = new Texture2D(1, 1);
  var loaderSettings = AsyncImageLoader.LoaderSettings.Default;
  var success = false;

  // =====================================
  // 将图像数据加载到现有纹理中
  // =====================================

  // 使用默认的LoaderSettings
  success = await AsyncImageLoader.LoadImageAsync(texture, imageData);

  // 类似于ImageConversion.LoadImage
  // 读取后将纹理标记为不可读。
  success = await AsyncImageLoader.LoadImageAsync(texture, imageData, true);

  // 使用自定义的LoaderSettings
  success = await AsyncImageLoader.LoadImageAsync(texture, imageData, loaderSettings);

  // ==================================
  // 从图像数据创建新纹理
  // ==================================

  // 使用默认的LoaderSettings
  texture = await AsyncImageLoader.CreateFromImageAsync(imageData);

  // 使用自定义的LoaderSettings
  texture = await AsyncImageLoader.CreateFromImageAsync(imageData, loaderSettings);
```

### 同步加载图像

同步版本的函数与异步版本相同，只是函数名中去掉了 `Async` 后缀。它们适用于在单帧内进行调试和性能分析。

```cs
  var imageData = File.ReadAllBytes();
  var texture = new Texture2D(1, 1);
  var loaderSettings = AsyncImageLoader.LoaderSettings.Default;
  var success = false;

  // =====================================
  // 将图像数据加载到现有纹理中
  // =====================================

  // 使用默认的LoaderSettings
  success = AsyncImageLoader.LoadImage(texture, imageData);

  // 类似于ImageConversion.LoadImage
  // 读取后将纹理标记为不可读。
  success = AsyncImageLoader.LoadImage(texture, imageData, true);

  // 使用自定义的LoaderSettings
  success = AsyncImageLoader.LoadImage(texture, imageData, loaderSettings);

  // ==================================
  // 从图像数据创建新纹理
  // ==================================

  // 使用默认的LoaderSettings
  texture = AsyncImageLoader.CreateFromImage(imageData);

  // 使用自定义的LoaderSettings
  texture = AsyncImageLoader.CreateFromImage(imageData, loaderSettings);
```

## 加载后

### 纹理格式

如果图像具有Alpha通道，格式将为 `RGBA32`，否则为 `RGB24`。

### Mipmap 数量

如果 `LoadImage` 和 `LoadImageAsync` 的 `generateMipmap` 设置为 `true`
，则Mipmap数量将设置为该纹理的最大可能值。如果你想控制Mipmap的数量，可以使用 `CreateFromImage` 和 `CreateFromImageAsync`。

### Mipmap 数据

Mipmap使用2x2核的盒式滤波生成。最终结果与在编辑器中使用纹理导入时的Unity默认结果不同。

## 故障排除

### 加载大图像时仍有卡顿

在 `AsyncImageLoader` 方法执行完毕后，图像数据仍在传输到GPU。因此，任何想要使用该纹理的对象（如材质或UI）都必须等待纹理上传完成，从而阻塞Unity的主线程。

目前没有简单的方法来检测纹理是否已完成上传。以下是一些解决方法：

+ 在使用纹理前等待一秒钟或更长时间。
+ **（未测试）** 使用 [`AsyncGPUReadback`](https://docs.unity3d.com/ScriptReference/Rendering.AsyncGPUReadback.html)
  从纹理中请求一个像素。它将等待纹理上传完成后才会下载该像素。然后可以使用请求回调来通知Unity主线程纹理上传已完成。

## 致谢

本包灵感来源于Matias Lavik的 [`unity-async-textureimport`](https://codeberg.org/matiaslavik/unity-async-textureimport)。