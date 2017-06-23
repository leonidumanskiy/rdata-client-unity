# RData - Data Collection Instrument

Unity Client

## Basic usage
Please, see Examples folder for example scenes.

## Setting up the project
To be able to build the dll, you need to add *UnityEngine.dll*, *UnityEngine.UI.dll* and *UnityEditor.dll* to the references.
To do that:

1. Open `RDataClassLibrary.sln` solution file
2. Open *Solution Explorer* by clicking View -> Solution Explorer
3. Expand *RDataClassLibrary* solution and project. Right click on *References*, and click *Add reference*
4. Click *Browse*, and then click on the *Browse* button. 
5. Navigate to your Unity Installation (`C:\Program Files\Unity\Editor\Data\Managed` on Windows, `/Applications/Unity/Unity.app/Contents/Frameworks/Managed/` on Mac). 
6. Select UnityEngine.dll
7. Repeat steps for UnityEditor.dll and UnityEngine.UI.dll (located in `Editor\Data\UnityExtensions\Unity\GUISystem`).

You can read more about building managed plugins on [Unity Website](https://docs.unity3d.com/Manual/UsingDLL.html).

## Compiling the RData.dll class library
To compile the class library, use the RDataClassLibrary.sln solution
