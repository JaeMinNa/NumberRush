using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    private AudioSource m_BGMAudioSource = null;
    private AudioSource m_PlayerSFXAuidoSource = null;
    private AudioSource[] m_EtcSFXAudioSources = new AudioSource[5];

    private Dictionary<string, AudioClip> m_BgmDic = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> m_SfxDic = new Dictionary<string, AudioClip>();
    private int m_SoundNum;
    private float m_MaxDistance = 50f;
    private float m_StartVolume = 1f;
    private bool m_IsInitialized = false;

    public static SoundManager Instance
    {
        get
        {
            if (m_Instance == null && Application.isPlaying)
            {
                GameObject Obj = GameObject.Find("[Managers]");
                if (Obj == null)
                {
                    Obj = new GameObject("[Managers]");
                    DontDestroyOnLoad(Obj);
                }

                GameObject managerObj = GameObject.Find("[Managers]/SoundManager");
                if (managerObj == null)
                {
                    managerObj = new GameObject("SoundManager");
                    managerObj.transform.SetParent(Obj.transform);
                }

                m_Instance = managerObj.GetComponent<SoundManager>();
                if (m_Instance == null)
                {
                    m_Instance = managerObj.AddComponent<SoundManager>();
                }

                m_Instance.CreateInstance();
            }

            return m_Instance;
        }
    }

    #region Override Method
    public override void DestroyInstance()
    {

    }

    protected override void CreateInstance()
    {

    }
    #endregion

    #region Public Method
    // 사운드가 거리에 따라 볼륨 조절이 필요할 때
    // 2D에서는 Vector2.Distance 사용
    //public void StartSFX(string name, Vector3 position)
    //{
    //    var MyCharacter = GameObject.Find($"{DataManager.Instance.GetMyUserData().UserHeroData.EquipHero.HeroName}(Clone)");
    //    if (MyCharacter == null)
    //        return;

    //    m_SoundNum = m_SoundNum % m_EtcSFXAudioSources.Length;

    //    float distance = Vector3.Distance(position, MyCharacter.transform.position);
    //    float volume = 1f - (distance / m_MaxDistance);
    //    m_EtcSFXAudioSources[m_SoundNum].volume = Mathf.Clamp01(volume) * m_StartVolume;
    //    m_EtcSFXAudioSources[m_SoundNum].PlayOneShot(m_SfxDic[name]);

    //    m_SoundNum++;
    //}

    public void SetSoundManager()
    {
        if (m_IsInitialized)
            return;

        m_BGMAudioSource = transform.AddComponent<AudioSource>();
        m_PlayerSFXAuidoSource = transform.AddComponent<AudioSource>();
        for (int index = 0; index < m_EtcSFXAudioSources.Length; ++index)
            m_EtcSFXAudioSources[index] = transform.AddComponent<AudioSource>();

        // AudioMixerGroup 불러오기
        AudioMixer mixer = ResourceLoader.LoadAsset<AudioMixer>("AudioMixer/AudioMixer", "AudioMixer");
        AudioMixerGroup[] bgmGroups = mixer.FindMatchingGroups("BGM");
        AudioMixerGroup[] sfxGroups = mixer.FindMatchingGroups("SFX");
        AudioMixerGroup bgmGroup = bgmGroups.Length > 0 ? bgmGroups[0] : null;
        AudioMixerGroup sfxGroup = sfxGroups.Length > 0 ? sfxGroups[0] : null;

        // BGM
        m_BGMAudioSource.loop = true;
        m_BGMAudioSource.volume = m_StartVolume;
        m_BGMAudioSource.outputAudioMixerGroup = bgmGroup;

        // SFX (Player)
        m_PlayerSFXAuidoSource.playOnAwake = false;
        m_PlayerSFXAuidoSource.volume = m_StartVolume;
        m_PlayerSFXAuidoSource.outputAudioMixerGroup = sfxGroup;

        // SFX (Etc)
        for (int index = 0; index < m_EtcSFXAudioSources.Length; ++index)
        {
            m_EtcSFXAudioSources[index].playOnAwake = false;
            m_EtcSFXAudioSources[index].volume = m_StartVolume;
            m_EtcSFXAudioSources[index].outputAudioMixerGroup = sfxGroup;
        }

        // BGM
        m_BgmDic.Add("BGM_Title", ResourceLoader.LoadAsset<AudioClip>("Sound/BGM/BGM_Title", "BGM_Title"));
        m_BgmDic.Add("BGM_Lobby", ResourceLoader.LoadAsset<AudioClip>("Sound/BGM/BGM_Lobby", "BGM_Lobby"));
        m_BgmDic.Add("BGM_Game", ResourceLoader.LoadAsset<AudioClip>("Sound/BGM/BGM_Game", "BGM_Game"));

        // SFX
        m_SfxDic.Add("ClickButton", ResourceLoader.LoadAsset<AudioClip>("Sound/SFX/UI/ClickButton", "ClickButton"));
        m_SfxDic.Add("MissButton", ResourceLoader.LoadAsset<AudioClip>("Sound/SFX/UI/MissButton", "MissButton"));
        m_SfxDic.Add("BuyButton", ResourceLoader.LoadAsset<AudioClip>("Sound/SFX/UI/BuyButton", "BuyButton"));
        m_SfxDic.Add("DestroyLine", ResourceLoader.LoadAsset<AudioClip>("Sound/SFX/Game/DestroyLine", "DestroyLine"));
        m_SfxDic.Add("FailButton", ResourceLoader.LoadAsset<AudioClip>("Sound/SFX/Game/FailButton", "FailButton"));
        m_SfxDic.Add("SuccessButton", ResourceLoader.LoadAsset<AudioClip>("Sound/SFX/Game/SuccessButton", "SuccessButton"));

        // 볼륨 설정
        float volume_SFX = PlayerPrefs.GetFloat("Volume_SFX");
        float volume_BGM = PlayerPrefs.GetFloat("Volume_BGM");

        if (volume_SFX == -40f)	// -40일 때, 음악을 꺼줌
        {
            mixer.SetFloat("SFX", -80f);
        }
        else
        {
            mixer.SetFloat("SFX", volume_SFX);
        }

        if (volume_BGM == -40f)	// -40일 때, 음악을 꺼줌
        {
            mixer.SetFloat("BGM", -80f);
        }
        else
        {
            mixer.SetFloat("BGM", volume_BGM);
        }

        m_IsInitialized = true;
    }

    // Player에서 출력되는 사운드
    public void StartSFX(string name)
    {
        if (!m_IsInitialized)
            return;

        m_PlayerSFXAuidoSource.PlayOneShot(m_SfxDic[name]);
    }

    public void StartBGM(string name)
    {
        if (!m_IsInitialized)
            return;

        m_BGMAudioSource.Stop();
        m_BGMAudioSource.clip = m_BgmDic[name];
        m_BGMAudioSource.Play();
    }

    public void StopBGM()
    {
        if (!m_IsInitialized)
            return;

        if (m_BGMAudioSource != null)
            m_BGMAudioSource.Stop();
    }
    #endregion
}