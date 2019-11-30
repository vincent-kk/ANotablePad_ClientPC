using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * UGUI 스크롤을 사용하는 경우에 자동으로 아이템을 추가하는 스크립트.
 * item프리팹을 전달받아서 이를 복사하여 생성한다.
 * 인스팩터에서 아이템의 오프셋과 자질구래한 것들을 설정해 주어야 한다.
 */
public class ScrollManager : MonoBehaviour
{
    [SerializeField] private GameObject item;
    [SerializeField] private RectTransform container;

    public float itemOffsetY;
    public float itemOffsetX;
    public float interItem;
    public int itemsInLine;
    public float defaultWidth;

    private float yOffset;
    private Vector2 nextItemPosition;
    private readonly Dictionary<string, Item> _dictionary = new Dictionary<string, Item>(10);

    private void InitScroll()
    {
        if (_dictionary.Count > 0) ClearScrollBox();
        yOffset = itemOffsetY + interItem;
        nextItemPosition = new Vector2(interItem, -interItem + yOffset);
    }
    public void AddItemsFromList(IEnumerable<string> list)
    {
        InitScroll();
        foreach (var b in list)
        {
            AddItem(b);
        }
    }
    private void AddItem(string b)
    {
        // 이미 존재하면 리턴
        if (_dictionary.ContainsKey(b)) return;

        var newBlock = Instantiate(item, container.transform);

        //newBlock 데이터를 밀어 넣는다....
        var newBlockComp = newBlock.GetComponent<Item>();

        newBlockComp.SetItemData(b);

        nextItemPosition = CalNextPosition(_dictionary.Count);
        _dictionary[b] = newBlockComp;
        newBlock.GetComponent<RectTransform>().anchoredPosition = nextItemPosition;
    }

    public void ClearScrollBox()
    {
        var childrenObjects = container.GetComponentsInChildren<Item>(false);

        if (childrenObjects == null) return;
        foreach (var obj in childrenObjects)
        {
            obj.DeleteItem();
        }
        container.sizeDelta = new Vector2(defaultWidth, interItem);
        _dictionary.Clear();
    }

    private Vector2 CalNextPosition(int itemCount)
    {
        if (itemCount % itemsInLine == 0)
        {
            container.sizeDelta += new Vector2(0, yOffset);
            return new Vector2(interItem, nextItemPosition.y - yOffset);
        }
        else
        {
            return new Vector2(nextItemPosition.x + interItem + itemOffsetX, nextItemPosition.y);
        }
    }
}