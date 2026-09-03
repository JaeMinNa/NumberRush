using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;


public class Popup_Settings : UIElement
{
    #region Cashed Object
    [SerializeField] private Slider Slider_SFX = null;
    [SerializeField] private Slider Slider_BGM = null;
    [SerializeField] private Image Img_Slider_SFX = null;
    [SerializeField] private Image Img_Slider_BGM = null;
    [SerializeField] private Button Btn_Close = null;
    [SerializeField] private Button Btn_Exit = null;
    #endregion

    #region Member Property
    private AudioMixer m_AudioMixer = null;
    #endregion

    #region Override Method
    public override void Init()
    { 
        if (m_AudioMixer == null)
            m_AudioMixer = ResourceLoader.LoadAssetResources<AudioMixer>("AudioMixer/AudioMixer");

        Btn_Close.onClick.AddListener(OnClick_Close);
        Btn_Exit.onClick.AddListener(OnClick_Exit);
    }

    public override void OnOpen(List<object> Args)
    {
        SetSlider();
    }

    public override void OnClose()
    {

    }

    public override void OnRefresh()
    {

    }
    #endregion

    #region Public Method
    public void SetSFX()
    {
        float sound = Slider_SFX.value;

        if (sound == -40f)	// -40일 때, 음악을 꺼줌
        {
            Img_Slider_SFX.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Slider/Slider_HandleType_Frame_Off");
            m_AudioMixer.SetFloat("SFX", -80f);
        }
        else
        {
            Img_Slider_SFX.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Slider/Slider_HandleType_Frame_On");
            m_AudioMixer.SetFloat("SFX", sound);
        }

        PlayerPrefs.SetFloat("Volume_SFX", sound);
    }

    public void SetBGM()
    {
        float sound = Slider_BGM.value;

        if (sound == -40f)	// -40일 때, 음악을 꺼줌
        {
            Img_Slider_BGM.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Slider/Slider_HandleType_Frame_Off");
            m_AudioMixer.SetFloat("BGM", -80f);
        }
        else
        {
            Img_Slider_BGM.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Slider/Slider_HandleType_Frame_On");
            m_AudioMixer.SetFloat("BGM", sound);
        }

        PlayerPrefs.SetFloat("Volume_BGM", sound);
    }
    #endregion

    #region Button Event

    private void SetSlider()
    {
        float volume_SFX = PlayerPrefs.GetFloat("Volume_SFX");
        float volume_BGM = PlayerPrefs.GetFloat("Volume_BGM");

        Slider_SFX.value = volume_SFX;
        Slider_BGM.value = volume_BGM;

        if (volume_SFX == -40f)
            Img_Slider_SFX.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Slider/Slider_HandleType_Frame_Off");

        if (volume_BGM == -40f)
            Img_Slider_BGM.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Slider/Slider_HandleType_Frame_Off");
    }

    private void OnClick_Close()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        UIManager.Instance.Close<Popup_Settings>();
    }

    private void OnClick_Exit()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        GameManager.Instance.ExitGame();
    }
    #endregion
}