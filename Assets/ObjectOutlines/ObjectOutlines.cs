using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
1 Render the scene to a texture(render target)
2 Render only the selected objects to another texture, in this case the capsule and box
3 Draw a rectangle across the entire screen and put the texture on it with a custom shader
4 The pixel/fragment shader for that rectangle will take samples from the previous texture, and add color to pixels which are near the object on that texture
5 Blur the samples
*/

/*
Step 1: Render the scene to a texture
attach it to the camera gameobject:
*/
[ExecuteInEditMode]
public class ObjectOutlines : MonoBehaviour 
{
    public string outlineLayer = "Outline";
    Camera AttachedCamera;
    public Shader Post_Outline;
    public Shader DrawSimple;
    Camera TempCam;
    Material Post_Mat;
 
    // void Start () 
    // {
    // }
    void OnEnable () {
        AttachedCamera = GetComponent<Camera>();
        TempCam = new GameObject().AddComponent<Camera>();
        TempCam.enabled = false;
        Post_Mat = new Material(Post_Outline);

    }

    void OnDisable () {
        if (Application.isPlaying) {
            DestroyImmediate(TempCam.gameObject);
            DestroyImmediate(Post_Mat);
        }
    }
 
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //maybe render temp cam and then fade out into clear instead of scene tex
        //use that image as ui overlay


        //set up a temporary camera
        TempCam.CopyFrom(AttachedCamera);
        TempCam.clearFlags = CameraClearFlags.Color;
        TempCam.backgroundColor = Color.black;
 
        //cull any layer that isn't the outline
        TempCam.cullingMask = 1 << LayerMask.NameToLayer(outlineLayer);
 
        //make the temporary rendertexture
        // RenderTexture TempRT = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.R8);
        RenderTexture TempRT = RenderTexture.GetTemporary(
            source.width, source.height, 0, RenderTextureFormat.R8
        );
        //put it to video memory
        TempRT.Create();
 
        //set the camera's target texture when rendering
        TempCam.targetTexture = TempRT;
 
        //render all objects this camera can render, but with our custom shader.
        TempCam.RenderWithShader(DrawSimple,"");


 
        Post_Mat.SetTexture("_SceneTex", source); 
        
        //copy the temporary RT to the final image
        

        //added
        // Graphics.Blit(source, destination);
        
        
        Graphics.Blit(TempRT, destination, Post_Mat);
 
        //release the temporary RT
        TempRT.Release();

 
    }
 
}
/*



REPORT THIS AD

And the final result:

FinalRender_blurred

Where you can go from here

First of all, the values, iterations, radius, color, etc are all hardcoded in the shader. You can set them as properties to be more code and designer friendly.

Second, my code has a falloff based on the number of filled texels in the area, and not the distance to each texel. You could create a gaussian kernel table and multiply the blurs with your values, which would run a little bit faster, but also remove the artifacts and uneven-ness you can see at the corners of the cube.

Don’t try to generate the gaussian kernel at runtime, though. That is super expensive.

I hope you learned a lot from this! If you have any questions or comments, please leave them below.

ADVERTISING


Advertisements

REPORT THIS AD
Share this:
TwitterFacebook

47 thoughts on “Unity Shaderlab: Object Outlines”

Lorenzo
April 16, 2016 at 5:13 pm
Thanks a lot for the useful information! Keep it up!

Like

Reply
Paul
May 18, 2016 at 10:35 pm
Hi! This is the first time I’ve found this explained in such detail, thanks!

I still don’t fully understand the shader’s code though, and I can’t get to work the blurred version, I’m getting this http://i.imgur.com/SwPubj3.png.

It seems that the alpha blending part of the code is not working, but I can’t find a way to fix it.

Any ideas? Thanks!

Like

Reply
William Weissman
May 31, 2016 at 10:09 am
In the final version of the shader(which I think is what you’re using), I removed the alpha blending line, and instead the shader just makes the color equal to whatever the other texture was. So, you need to pass in the existing scene texture to _SceneTex. You can do this from C# by setting the material’s texture via Post_Mat.SetTexture(“_SceneTex”,TheSceneTexture);.

To make that scene texture you’ll likely have to make a copy of TempRT before running the outline shader, and pass that in as the last parameter.

Like

Reply
Paul
May 31, 2016 at 8:04 pm
Thanks for your answer! I’ve just tried doing that, but I can’t get it to work, I’m pretty sure that the only thing missing is how to get the current scene texture, but it seems that I’m doing it wrong. I’ve tried creating a copy of the tempRT, just as you explained, but it’s still rendering a black scene.

Like

Tobias
June 2, 2016 at 2:03 am
I’m having the same problems as Paul. My best guess is that there is a missing function that blends the scene texture with the main texture inside the shader code. Passing in the SceneTexture doesn’t change anything on the final rendered image. Please advise. 🙂

Like

Dave
June 6, 2016 at 6:18 pm
Superb tutorial, thanks for making it!

However, I am having the same issue as the other two. I experimented with the .SetTexture solution but ran into the problem I had to flip the scene RT on the Y axis, and despite my best efforts, it quickly turned into a mess. Any input would be appreciated.

Like


REPORT THIS AD

Dmitry
July 5, 2016 at 8:09 am
Are you insane to create new render texture each frame? the cpu and memory just wasted. the only case it’s required is the screen resolution changed

Like

Reply
William Weissman
July 5, 2016 at 9:22 am
Definitely a good point. I’ll have to look into optimizing that part once I fix up the tutorial.

Like

Reply
Rabbitator
July 6, 2016 at 8:00 am
Please do it today I’m begging you! I really need this shader with blur, but don’t know how to fix it.

Like

adam (@ADAMATOMIC)
November 4, 2016 at 9:38 am
afaict Unity actually suggests using this exact method for temporary render textures, since its using an object pool behind the scenes, and it allows that pool to be shared between other fullscreen image fx

also will thank you so much for this tutorial, it was a HUGE help for Overland!

Like


REPORT THIS AD

ThatHg
July 13, 2016 at 2:41 am
I tried to follow your tutorial but I only get a black screen. I’m using Unity 5.3.4f1

Like

Reply
ThatHg
July 13, 2016 at 2:53 am
Got an outline working but i cant get your tip about using Post_Mat.SetTexture(“_SceneTex”,TheSceneTexture); to render the scene to work.

Liked by 1 person

Reply
Adolar
June 7, 2017 at 6:58 am
Hey, also getting just a black Screen on Unity 5.6.1f1, how’d you fix it?

Like

Reply
ThatHg
July 13, 2016 at 4:23 am
Ok, So i got the scene working now, but somehow your shader only works with conjunction with my old, “wrong” outline shader that offets normals and stuff. http://i.imgur.com/HLCxwhk.png the plain cube is using unity default shader. To get the scene working I added this Post_Mat.SetTexture(“_SceneTex”, source); before TempCamp.RenderWithShader… Then in shader i added
scene = (1 – ColorIntensityInRadius) * scene;
//output some intensity of teal
return ColorIntensityInRadius*half4(1,0,0,1) + scene;

where scene is half4 scene = tex2D(_SceneTex, float2(i.uvs.x, i.uvs.y));

Like

Reply
JonTheCoder
January 21, 2018 at 9:29 am
1.5 years later, still relevant. Thank you so much!

This solution is what worked for me, Unity 2017.3.0f3. (After deleting all the return characters and putting in new ones in the shader because copying and pasting from the website ‘corrupts’ it all…. ugh)

Keep in mind the author of this fix above renamed some variables.

Like

Reply

REPORT THIS AD

bombambini
July 31, 2016 at 7:05 pm
how to i change the color?

Like

Reply
Matthew Laverick
September 6, 2016 at 9:15 am
Hi,

Thanks for the write up.
For most part, this works great! Unfortunately it doesn’t work in a WebGL build for me. The log says that the shader is incompatible because it doesn’t have a vertex shader pass.
I don’t know a thing about shader code, but would it be possible to make this compatible with WebGL?
Am I also right in thinking that this only works in Forward Rendering?

Thanks,

Matt

Like

Reply
JV
September 28, 2016 at 9:02 am
Hey there,
thanks for the tutorial, helped me a lot and was actually the first useful explanation on the topic i could find.
anyway, its working perfectly fine in the unity editor, but when i export to webgl the screen turns pink when the shader should do its thing.
I guess this is a compability issue with webgl (wouldnt be the first one…) or is there anything else i may have done wrong? i added the shaders to the always include list in the graphics settings.
any ideas?

Like

Reply

REPORT THIS AD

Justin
November 29, 2016 at 3:52 pm
Hey Will,

Fantastic resource! I’ll echo some of the above sentiments on thoughts for WebGL support — it only renders the Simple pass. I was able to get it working fine in editor by trying a different approach to the Blit. This keeps the screen from going black, but then the outline will no longer work (in WebGL).

Graphics.Blit(TempRT, source, Post_Mat);
Graphics.Blit(source, destination);

Any thoughts are appreciated!

Like

Reply
Justin
November 29, 2016 at 4:32 pm
Scratch that! Replacing the Blit with the lines above + changing the RenderTexture format to Default fixed it — R8 is unsupported in WebGL.

RenderTexture TempRT = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.Default);

Thanks again for the great resource 🙂

Like

Reply
Xib
November 30, 2016 at 4:26 am
For anyone having problems having a black screen with the blurred version.
You have to set the texture like this http://imgur.com/8HXwxxp.png in the OnRenderImage method.

Afterwards you will see that everything is inverted there for you have to remove these http://i.imgur.com/cFzTvGH.png in the outline shade in order to remove the invertion.

Final result should be http://i.imgur.com/yNn1KBl.png

Really awesome tutorial Will Weissman this helped me a lot.

Like

Reply
Jan Maciolka
December 12, 2018 at 4:47 am
❤ Man! You rocks!
Thanks for solve that problem!

Like

Reply

REPORT THIS AD

JonAM
December 20, 2016 at 6:21 am
Useful indeed. Thanks for sharing.

Like

Reply
Jon
February 9, 2017 at 6:41 am
Great resource and information on how to accomplish outlining in your own project.

We need more people like you, that take the time to provide this valuable information to the community. Very much appreciated.

Like

Reply
deniz
February 17, 2017 at 4:06 am
Great informative post! Thank you for sharing.

If anybody wants to implement this on Steam VR, simply put the post effect script on Camera(eye). In my case the last rendered image was flipped. I simply attached one more SteamVR_CameraFlip script to Camera(eye). I hope this helps someone out there. I made a short post about it, if anybody would need a bit more information: https://denizbicer.github.io/2017/02/16/Object-Outlines-at-Steam-VR-with-Unity-5.html

Like

Reply
Object Outlines at Steam VR with Unity 5 – A Journey of a Mixed Reality Developer
February 26, 2017 at 5:51 pm
[…] have found this amazing tutorial to create outlines to objects. It works like a charm in a Unity project, where there is only one […]

Liked by 1 person

Reply

REPORT THIS AD

greggman
March 13, 2017 at 1:24 am
Thank you for this great explanation! So helpful!

Like

Reply
Andrea
March 16, 2017 at 4:03 pm
Have you a suggestion on how to achieve the occlusion of the outline if an object is in front of the outlined one?﻿
Thanks!

Like

Reply
p216516
April 12, 2017 at 4:33 am
Hey I’m trying to do the same thing here.

How to pass the depth buffer from the camera rendering the entire scene to the one rendering the highlighted objects white on black? (which would still lead to the issue of flickering, but at least that would bring me one step closer)

The next step would be to use the stencil buffer from the scene rendering to create the black and white image instead of rendering the highlighted object.

Help please =)

Like

Reply
李建邦
October 31, 2017 at 10:29 pm
Some ideas:
1. Draw highlighted objects as red, other things in the scene in blue using replacement shaders
2. Draw the outline but not only discard pixels whose r channel != 0, but also check for b channel, too.

Like

Reply

REPORT THIS AD

Reeven
May 31, 2017 at 10:43 am
I would like to know the same thing as Andrea : how to occlude the outline should the outlined object be behind another. Basically, I only want to implement a selection outline, not x-rays.

The Unity Wiki tutorial achieves this by commenting out the “Ztest Always” code in the passes, but your own method is architectured differently and I am not too sure where to tweak that feature.

Like

Reply
Adolar
June 7, 2017 at 7:00 am
Hey, I just got a black screen after Step 2. Any thoughts?

Like

Reply
Mike
July 1, 2017 at 2:06 pm
I think I found the solution. In the SimpleDraw shader on line 18, replace float4 pos:SV_POSITION with float4 pos:POSITION;

Like

Reply
Mike
June 29, 2017 at 7:20 am
Hi, this looks like a really great tutorial, but unfortunately I’m falling at the first hurdle.

I’m using Unity 5.6.2f1 and after setting up Stage 1 I see a solid red screen.

The temporary camera is being created & is showing only objects on the Outline layer, so the problem seems to be with the RenderTexture.

Any ideas?

Like

Reply

REPORT THIS AD

House
August 1, 2017 at 3:21 pm
I’m getting the error: “Assertion failed: Invalid mask passed to GetVertexDeclaration()” on the line Graphics.Blit(TempRT, destination, Post_Mat);

It doesn’t seem to like the Post_Mat for some reason (when I took out just that parameter in blit the error goes away, but it doesn’t have the intended effect anymore)

Like

Reply
Adam Clements
September 15, 2017 at 11:01 am
I was getting just the outline on a black background, I figured at no point are we blitting the original source to the destination, so I added one line

//copy the temporary RT to the final image
Graphics.Blit(source, destination);
Graphics.Blit(TempRT, destination,Post_Mat);

And then it works perfectly with no other changes. Hope that helps anyone who was having the same trouble as me – fantastic tutorial, thanks! An excellent base to work from for what I want to do.

Like

Reply
Programmer's Digest
October 8, 2017 at 12:01 pm
Good tutorial, just wish it was updated as there are a few minor glitches. As stated by Will above, _SceneTex has to be provided to the shader in order to show more than a black screen. The image will then, however be flipped. This is due to two 1-i.uvs.y too many in the final Post Outline shader:
Line 130 should read:
return tex2D(_SceneTex,float2(i.uvs.x,i.uvs.y))

Line 149 should read:
half4 outcolor = ColorIntensityInRadius*half4(0,1,1,1)*2+(1-ColorIntensityInRadius)*tex2D(_SceneTex,float2(i.uvs.x,i.uvs.y));

Do keep the 1-i.uvs.y on line 139 though, otherwise the outline will be flipped.

Like

Reply

REPORT THIS AD

akulahalfi
November 4, 2017 at 6:03 am
How do we achieve the occlusion of the outline if an object is in front of the outlined object?

Like

Reply
kim
March 12, 2018 at 8:59 pm
I don’t know it’s a good idea, but it can be solved by installing three cameras.
1 : Background camera
2 : The Highlight Camera
3 : Object camera

Like

Reply
William Weissman
March 15, 2018 at 10:37 am
Actually, this is not a good idea. You’ll need to invest time into keeping all cameras consistent and synchronized with each other, on top of any other rendering overhead that comes with a camera.

Still, you’re essentially following what’s going on! The utility in using OnRenderImage() is that you can intercept the right points of the rendering pipeline on a single camera, and then you can easily apply the script to any camera in any scene without doing weird camera setups 🙂

Like

Alex_ADEdge
March 15, 2018 at 9:44 am
Solid tutorial – thanks!

My main issue in the end was the render texture being inverted on the Y axis, which I found comes down to a cross platform rendering issue, adding this code into the top of the fragment function in the PostOutline.shader fixes it:

#if UNITY_UV_STARTS_AT_TOP
if (_MainTex_TexelSize.y > 0)
i.uvs.y = 1 – i.uvs.y;
#endif

Solution from here: https://docs.unity3d.com/Manual/SL-PlatformDifferences.html



Will look in future at working out how to occlude things that move in front but for now it does exactly what I need.

Liked by 1 person

Reply
William Weissman
March 15, 2018 at 10:54 am
If I could pin this comment, I would

Liked by 1 person

Reply
SuperScience
October 6, 2018 at 9:47 pm
Hi,

For me, the outline is working, but nothing else. I just have some outlines on a black screen. Similar to what Adam Clements saw. I tried his double Blit idea, but it didn’t solve the problem.

Any idea what I am doing wrong? Thanks!

Like

Reply
SuperScience
October 7, 2018 at 12:46 am
Update: I was able to get it working by trying some of the other stuff explained above. However, I am now having the issue that Alex ADEdge was talking about where the outline seems to be offset (inverted) along the Y axis.

I tried adding his code snippit to the “top of the fragment function” in PostOutline.shader. However, I’m not sure where to put it. There are 2 fragment functions, if I put it in the top one, it seems to cull the wrong pixels. If I put it in the bottom one, I also need to add the float2 _MainTex_TexelSize; and it appears to have no effect.

Any help would be much appreciated!

Like

Reply
SuperScience
October 7, 2018 at 1:32 am
One more update… I’m starting to wonder if this system is working fine and the source, being fed into Post_Mat via this line: Post_Mat.SetTexture(“_SceneTex”, source); … is inverting the SOURCE image.

Like

SuperScience
October 7, 2018 at 3:09 am
Final update. Sorry for the spam. I was able to figure it out.

_SceneTex was being inverted in the shader, and yet the selection outline was not. I was able to fix it by removing the 1-i wherever the _SceneTex was being used.

Like

FeldonDragon
December 12, 2018 at 4:50 am
Awesome work Will! Appreciate that!
@Xib added solution to black screen with blured ver. and together you helped me a lot!
Thank you two! ^^

 */
