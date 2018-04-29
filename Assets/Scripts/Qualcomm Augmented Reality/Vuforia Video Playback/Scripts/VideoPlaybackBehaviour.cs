/*==============================================================================
/*============================================================================== 
 * Copyright (c) 2012-2014 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * Confidential and Proprietary – Qualcomm Connected Experiences, Inc. 
 * ==============================================================================*/

using UnityEngine;
using Vuforia;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// The VideoPlaybackBehaviour manages the appearance of a video that can be superimposed on a target.
/// Playback controls are shown on top of it to control the video. 
/// </summary>
public class VideoPlaybackBehaviour : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES

    /// <summary>
    /// URL of the video, either a path to a local file or a remote address
    /// </summary>    
    [HideInInspector]
    public string m_path = null;

    #endregion // PUBLIC_MEMBER_VARIABLES



    #region PRIVATE_MEMBER_VARIABLES

    private VideoPlayerHelper mVideoPlayer = null;
    private bool mIsInited = false;
    private bool mIsPrepared = false;
    private bool mAppPaused = false;

    private Texture2D mVideoTexture = null;


    private Texture mKeyframeTexture = null;

    private VideoPlayerHelper.MediaType mMediaType =
            VideoPlayerHelper.MediaType.ON_TEXTURE_FULLSCREEN;

    private VideoPlayerHelper.MediaState mCurrentState =
            VideoPlayerHelper.MediaState.NOT_READY;

    private float mSeekPosition = 0.0f;

    private bool isPlayableOnTexture;



    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PROPERTIES

    /// <summary>
    /// Returns the video player
    /// </summary>
    public VideoPlayerHelper VideoPlayer
    {
        get { return mVideoPlayer; }
    }

    /// <summary>
    /// Returns the current playback state
    /// </summary>
    public VideoPlayerHelper.MediaState CurrentState
    {
        get { return mCurrentState; }
    }

    /// <summary>
    /// Type of playback (on-texture only, fullscreen only, or both)
    /// </summary>
    public VideoPlayerHelper.MediaType MediaType
    {
        get { return mMediaType; }
        set { mMediaType = value; }
    }

    /// <summary>
    /// Texture displayed before video playback begins
    /// </summary>
    public Texture KeyframeTexture
    {
        get { return mKeyframeTexture; }
        set { mKeyframeTexture = value; }
    }



    #endregion // PROPERTIES



    #region UNITY_MONOBEHAVIOUR_METHODS


    void Awake()
    {
        mVideoPlayer = new VideoPlayerHelper();
        transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x),
               transform.localScale.y, transform.localScale.z);   
    }



    public void SetPath(string path)
    {
        // Find the icon plane (child of this object)
        //mIconPlane = transform.Find("Icon").gameObject;        

        // A filename or url must be set in the inspector        
        m_path = path;
        mIsInited = false;
        mIsPrepared = false;
        mAppPaused = false;
        if (m_path == null || m_path.Length == 0)
        {            
            HandleStateChange(VideoPlayerHelper.MediaState.ERROR);
            mCurrentState = VideoPlayerHelper.MediaState.ERROR;
            this.enabled = false;
        }
        else
        {
            // Set the current state to Not Ready
            HandleStateChange(VideoPlayerHelper.MediaState.NOT_READY);
            mCurrentState = VideoPlayerHelper.MediaState.NOT_READY;
        }
        // Create the video player and set the filename        
        mVideoPlayer.SetFilename(m_path);
    }

    bool inited = false;

    

    public void Update()
    {
        if (mAppPaused || m_path == null || m_path == "") return;

        if (!mIsInited)
        {
            // Initialize the video player
            if (!inited && mVideoPlayer.Init() == false)
            {
                HandleStateChange(VideoPlayerHelper.MediaState.ERROR);
                this.enabled = false;
                inited = true;
                return;
            }

            // Initialize the video texture
            InitVideoTexture();
            mMediaType = VideoPlayerHelper.MediaType.ON_TEXTURE;

            // Load the video
            if (mVideoPlayer.Load(m_path, mMediaType, false, 0) == false)
            {
                Debug.Log("Could not load video '" + m_path + "' for media type " + mMediaType);
                HandleStateChange(VideoPlayerHelper.MediaState.ERROR);
                this.enabled = false;
                return;
            }

            // Successfully initialized
            mIsInited = true;
        }
        else if (!mIsPrepared)
        {
            // Get the video player status
            VideoPlayerHelper.MediaState state = mVideoPlayer.GetStatus();

            if (state == VideoPlayerHelper.MediaState.ERROR)
            {
                Debug.Log("Could not load video '" + m_path + "' for media type " + mMediaType);
                HandleStateChange(VideoPlayerHelper.MediaState.ERROR);
                this.enabled = false;
            }
            else if (state < VideoPlayerHelper.MediaState.NOT_READY)
            {
                // Video player is ready

                // Can we play this video on a texture?
                isPlayableOnTexture = mVideoPlayer.IsPlayableOnTexture();

                if (isPlayableOnTexture)
                {
                    // Pass the video texture id to the video player
                    // TODO: GetNativeTextureID() call needs to be moved to Awake method to work with Oculus SDK if MT rendering is enabled
                    int nativeTextureID = mVideoTexture.GetNativeTextureID();
                    mVideoPlayer.SetVideoTextureID(nativeTextureID);

                    if (mSeekPosition > 0)
                    {
                        mVideoPlayer.SeekTo(mSeekPosition);
                    }
                }
                else
                {
                    // Handle the state change
                    state = mVideoPlayer.GetStatus();
                    HandleStateChange(state);
                    mCurrentState = state;
                }

                // Scale the icon
                ScaleIcon();

                // Video is prepared, ready for playback
                mIsPrepared = true;
            }
        }
        else
        {
            if (isPlayableOnTexture)
            {
                // Update the video texture with the latest video frame
                VideoPlayerHelper.MediaState state = mVideoPlayer.UpdateVideoData();
                if ((state == VideoPlayerHelper.MediaState.PLAYING)
                    || (state == VideoPlayerHelper.MediaState.PLAYING_FULLSCREEN))
                {
                    GL.InvalidateState();
                }


                // Check for playback state change
                if (state != mCurrentState)
                {
                    HandleStateChange(state);
                    mCurrentState = state;
                }
            }
            else
            {
                // Get the current status
                VideoPlayerHelper.MediaState state = mVideoPlayer.GetStatus();
                if ((state == VideoPlayerHelper.MediaState.PLAYING)
                   || (state == VideoPlayerHelper.MediaState.PLAYING_FULLSCREEN))
                {
                    GL.InvalidateState();
                }

                // Check for playback state change
                if (state != mCurrentState)
                {
                    HandleStateChange(state);
                    mCurrentState = state;
                }
            }
        }
    }

    /*
    void OnApplicationPause(bool pause)
    {
        mAppPaused = pause;

        if (!mIsInited)
            return;

        if (pause)
        {
            // Handle pause event natively
            mVideoPlayer.OnPause();

            // Store the playback position for later
            mSeekPosition = mVideoPlayer.GetCurrentPosition();

            // Deinit the video
            mVideoPlayer.Deinit();

            // Reset initialization parameters
            mIsInited = false;
            mIsPrepared = false;

            // Set the current state to Not Ready
            HandleStateChange(VideoPlayerHelper.MediaState.NOT_READY);
            mCurrentState = VideoPlayerHelper.MediaState.NOT_READY;
        }
    }*/


    void OnDestroy()
    {
        // Deinit the video
        mVideoPlayer.Deinit();
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS





    #region PRIVATE_METHODS

    // Initialize the video texture
    private void InitVideoTexture()
    {
        // Create texture of size 0 that will be updated in the plugin (we allocate buffers in native code)
        if (mVideoTexture == null)
        {
            mVideoTexture = new Texture2D(0, 0, TextureFormat.RGB565, false);
            mVideoTexture.filterMode = FilterMode.Bilinear;
            mVideoTexture.wrapMode = TextureWrapMode.Clamp;
        }
    }


    public Action readyCall;
    public Action completeCall;
    public Action errorCall;
    public bool AutoPlay=false;

    // Handle video playback state changes
    private void HandleStateChange(VideoPlayerHelper.MediaState newState)
    {
        // If the movie is playing or paused render the video texture
        // Otherwise render the keyframe
        if (newState == VideoPlayerHelper.MediaState.PLAYING ||
            newState == VideoPlayerHelper.MediaState.PAUSED)
        {
            Material mat = GetComponent<Renderer>().material;
            mat.mainTexture = mVideoTexture;
            mat.mainTextureScale = new Vector2(1, 1);
        }
        else
        {
            if (mKeyframeTexture != null)
            {
                Material mat = GetComponent<Renderer>().material;
                mat.mainTexture = mKeyframeTexture;
                mat.mainTextureScale = new Vector2(1, -1);
            }
        }


        // Display the appropriate icon, or disable if not needed
        switch (newState)
        {
            case VideoPlayerHelper.MediaState.READY:
                if (readyCall != null) readyCall();
                break;
            case VideoPlayerHelper.MediaState.REACHED_END:
                if (completeCall != null) completeCall();
                break;
            case VideoPlayerHelper.MediaState.PAUSED:
            case VideoPlayerHelper.MediaState.STOPPED:
                break;
            case VideoPlayerHelper.MediaState.NOT_READY:
            case VideoPlayerHelper.MediaState.PLAYING_FULLSCREEN:
                break;
            case VideoPlayerHelper.MediaState.ERROR:
                if (errorCall != null) errorCall();
                break;
        }

        if (newState == VideoPlayerHelper.MediaState.PLAYING_FULLSCREEN)
        {
            // Switching to full screen, disable QCARBehaviour (only applicable for iOS)
            QCARBehaviour qcarBehaviour = (QCARBehaviour)FindObjectOfType(typeof(QCARBehaviour));
            qcarBehaviour.enabled = false;
        }
        else if (mCurrentState == VideoPlayerHelper.MediaState.PLAYING_FULLSCREEN)
        {
            // Switching away from full screen, enable QCARBehaviour (only applicable for iOS)
            QCARBehaviour qcarBehaviour = (QCARBehaviour)FindObjectOfType(typeof(QCARBehaviour));
            qcarBehaviour.enabled = true;

            StartCoroutine(ResetToPortraitSmoothly());
        }
    }

    private IEnumerator ResetToPortraitSmoothly()
    {
        Screen.autorotateToPortrait = true;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortraitUpsideDown = false;

        if (Screen.orientation == ScreenOrientation.Portrait)
        {
            // We need to trigger a screen orientation "reset", 
            // so, first we set it temporarily to landscape
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            // we wait for end of frame
            yield return new WaitForEndOfFrame();

            // we wait for about half a second to be sure the 
            // screen orientation has switched from portrait to landscape
            yield return new WaitForSeconds(1.0f);

            // then finally we reset to Portrait
            Screen.orientation = ScreenOrientation.Portrait;
        }
        else
        {
            // the screen orientation was already != Portrait
            // so we can reset to Portrait immediately
            Screen.orientation = ScreenOrientation.Portrait;
        }
    }

    private void ScaleIcon()
    {
        // Icon should fill 50% of the narrowest side of the video
        /*
       float videoWidth = Mathf.Abs(transform.localScale.x);
       float videoHeight = Mathf.Abs(transform.localScale.z);
       float iconWidth, iconHeight;

       if (videoWidth > videoHeight)
       {
           iconWidth = 0.5f * videoHeight / videoWidth;
           iconHeight = 0.5f;
       }
       else
       {
           iconWidth = 0.5f;
           iconHeight = 0.5f * videoWidth / videoHeight;
       }*/
    }



    #endregion // PRIVATE_METHODS
}
