/*============================================================================== 
 * Copyright (c) 2012-2014 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System.Collections;
using Vuforia;

/// <summary>
/// Demonstrates how to play the video on texture and full-screen mode.
/// Single tapping on texture will play the video on texture (if the 'Play FullScreen' Mode in the UIMenu is turned off)
/// or play full screen (if the option is enabled in the UIMenu)
/// At any time during the video playback, it can be brought to full-screen by enabling the options from the UIMenu.
/// </summary>
public class PlayVideo : MonoBehaviour
{

    private bool mVideoIsPlaying;
    VideoPlaybackBehaviour currentVideo;

    #region UNITY_MONOBEHAVIOUR_METHODS

    void Start()
    {
        currentVideo = GetComponent<VideoPlaybackBehaviour>();
    }

    void FixedUpdate()
    {
        if (Input.anyKeyDown)
        {
            HandleSingleTap();
        }
    }



    #endregion UNITY_MONOBEHAVIOUR_METHODS

    #region PRIVATE_METHODS


    /// <summary>
    /// Handle single tap event
    /// </summary>
    private void HandleSingleTap()
    {
        if (currentVideo != null)
        {    
                if (currentVideo.VideoPlayer.IsPlayableOnTexture())
                {
                    VideoPlayerHelper.MediaState state = currentVideo.VideoPlayer.GetStatus();
                    if (state == VideoPlayerHelper.MediaState.PAUSED ||
                        state == VideoPlayerHelper.MediaState.READY ||
                        state == VideoPlayerHelper.MediaState.STOPPED)
                    {
                        currentVideo.VideoPlayer.Play(false, currentVideo.VideoPlayer.GetCurrentPosition());
                    }
                    else if (state == VideoPlayerHelper.MediaState.REACHED_END)
                    {
                        currentVideo.VideoPlayer.Play(false, 0);
                    }
                    else if (state == VideoPlayerHelper.MediaState.PLAYING)
                    {
                        currentVideo.VideoPlayer.Pause();
                    }
                }
        }
    }
    
    public static IEnumerator PlayFullscreenVideoAtEndOfFrame(VideoPlaybackBehaviour video)
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        yield return new WaitForEndOfFrame ();

        // we wait a bit to allow the ScreenOrientation.AutoRotation to become effective
        yield return new WaitForSeconds (0.3f);
        
        video.VideoPlayer.Play(true, 0);
    }

    //Flash turns off automatically on fullscreen videoplayback mode, so we need to update the UI accordingly
    private void UpdateFlashSettingsInUIView()
    {
        VideoPlaybackUIEventHandler handler = GameObject.FindObjectOfType(typeof(VideoPlaybackUIEventHandler)) as VideoPlaybackUIEventHandler;
        if (handler != null)
        {
            handler.View.mCameraFlashSettings.Enable(false);
        }
    }

    /// <summary>
    /// Checks to see if the 'Play FullScreen' Mode is enabled/disabled in the UI Menu
    /// </summary>
    /// <returns></returns>
    private bool IsFullScreenModeEnabled()
    {
        VideoPlaybackUIEventHandler handler = FindObjectOfType(typeof(VideoPlaybackUIEventHandler)) as VideoPlaybackUIEventHandler;
        if (handler != null)
        {
            //return handler.mFullScreenMode;
        }

        return false;
    }

    /// <summary>
    /// Find the video object under the screen point
    /// </summary>
    private VideoPlaybackBehaviour PickVideo(Vector3 screenPoint)
    {
        VideoPlaybackBehaviour[] videos = (VideoPlaybackBehaviour[])
                FindObjectsOfType(typeof(VideoPlaybackBehaviour));
      
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);

        RaycastHit hit = new RaycastHit();

        foreach (VideoPlaybackBehaviour video in videos)
        {
            if (video.GetComponent<Collider>().Raycast(ray, out hit, 10000))
            {
                return video;
            }
        }

        return null;
    }


    #endregion // PRIVATE_METHODS

}
