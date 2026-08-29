using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using yjs.DevKit.UI;

public class ScrollView_BottomToUp : BaseScrollView
{
    protected override void OnValueChanged_Scroll(Vector2 pos)
    {
        var contentPos = GetScrollContentsPosition();
        var pageNo = Mathf.FloorToInt(contentPos.y / ScrollItem.ItemSize.y);
        var count = Mathf.Abs(pageNo - PrevPageNo);

        if (pageNo < PrevPageNo && pageNo >= 0)
        {
            var lastIndex = (int)ScrollItemList.LastOrDefault()?.ItemIndex;
            if (lastIndex < createCount)
            {
                for (int i = 0; i < count; i++)
                {
                    //  ����
                    for (int line = 0; line < LineRow; line++)
                    {
                        var prevItem = ScrollItemList.FirstOrDefault();
                        var checkSize = (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);

                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport, checkSize, Camera.main) == false)
                        {
                            scrollPool.PushItem(prevItem);
                            ScrollItemList.RemoveAt(0);
                        }
                    }

                    //  ����
                    for (int line = 0; line < LineRow; line++)
                    {
                        var nextIndex = lastIndex + 1;
                        if (nextIndex >= createCount)
                        {
                            break;
                        }

                        var no = ++lastIndex;
                        var nextItem = RepositionItem(ScrollDataList[no], no, scrollType);
                        ScrollItemList.Add(nextItem);

                        nextItem.gameObject.SetActive(true);
                    }
                }

                PrevPageNo = pageNo >= 0
                    ? pageNo
                    : 0;
            }
        }

        if (pageNo > PrevPageNo && pageNo <= MaxPageNo)
        {
            var firstIndex = (int)ScrollItemList.FirstOrDefault()?.ItemIndex;
            if (firstIndex >= 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var tempMax = ScrollItemList.LastOrDefault().ItemIndex == createCount - 1
                        ? createCount % LineRow
                        : LineRow;

                    //  ����
                    for (int line = 0; line < tempMax; line++)
                    {
                        var prevItem = ScrollItemList.LastOrDefault();
                        var checkSize = (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);
                        checkSize.y -= prevItem.ItemSize.y;
                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport, checkSize, Camera.main) == false)
                        {
                            scrollPool.PushItem(prevItem);
                            ScrollItemList.RemoveAt(ScrollItemList.Count - 1);
                        }
                    }

                    //  ����
                    for (int line = 0; line < LineRow; line++)
                    {
                        if (firstIndex <= 0)
                        {
                            break;
                        }

                        var no = --firstIndex;
                        var nextItem = RepositionItem(ScrollDataList[no], no, scrollType);
                        ScrollItemList.Insert(0, nextItem);

                        nextItem.gameObject.SetActive(true);
                    }
                }

                PrevPageNo = pageNo > MaxPageNo
                    ? MaxPageNo
                    : pageNo;
            }
        }
    }
}
