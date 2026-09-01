using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Slot_Number : MonoBehaviour
{
    [SerializeField] private GameObject Obj_BaseAdd = null;
    [SerializeField] private GameObject Obj_BaseLock = null;
    [SerializeField] private TMP_Text Text_Number = null;
    [SerializeField] private GameObject Obj_IconAdd = null;
    [SerializeField] private GameObject Obj_IconLock = null;
    [SerializeField] private GameObject Obj_Select = null;
    [SerializeField] private Button Btn_Slot = null;


    public void SetSlot(SlotType type, int num = -1, Action action = null)
    {
        InitSlot();

        // 전달받은 함수가 있으면 연결
        if (action != null)
        {
            Btn_Slot.onClick.AddListener(() => action());
        }

        switch (type)
        {
            case SlotType.Normal:

                Text_Number.text = num.ToString();
                Text_Number.transform.gameObject.SetActive(true);

                break;

            case SlotType.Add:

                Obj_BaseAdd.SetActive(true);
                Obj_IconAdd.SetActive(true);

                break;

            case SlotType.Lock:

                Obj_BaseLock.SetActive(true);
                Obj_IconLock.SetActive(true);

                Color color = Text_Number.color;
                color.a = 100f / 255f;
                Text_Number.color = color;

                Text_Number.text = num.ToString();
                Text_Number.transform.gameObject.SetActive(true);

                break;

            case SlotType.Select:

                Obj_Select.SetActive(true);

                Text_Number.text = num.ToString();
                Text_Number.transform.gameObject.SetActive(true);

                break;

            case SlotType.Equip:

                Text_Number.text = num.ToString();
                Text_Number.transform.gameObject.SetActive(true);
                Obj_BaseLock.SetActive(true);

                break;

            default:
                break;
        }
    }

    private void InitSlot()
    {
        // 버튼 이벤트 초기화
        Btn_Slot.onClick.RemoveAllListeners();

        Obj_BaseAdd.SetActive(false);
        Obj_BaseLock.SetActive(false);
        Text_Number.transform.gameObject.SetActive(false);
        Obj_IconAdd.SetActive(false);
        Obj_IconLock.SetActive(false);
        Obj_Select.SetActive(false);

        Color color = Text_Number.color;
        color.a = 1f;
        Text_Number.color = color;
    }
}
