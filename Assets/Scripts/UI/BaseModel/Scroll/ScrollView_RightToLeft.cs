using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using yjs.DevKit.UI;
    
public class ScrollView_RightToLeft : BaseScrollView
{
    protected override void OnValueChanged_Scroll(Vector2 pos)
    {
        var contentPos = GetScrollContentsPosition();

        var pageNo = Mathf.Abs(Mathf.FloorToInt(contentPos.x / ScrollItem.ItemSize.x));
        var count = Mathf.Abs(pageNo - PrevPageNo);

        var firstIndex = (int)ScrollItemList.FirstOrDefault().ItemIndex;
        var lastIndex = (int)ScrollItemList.LastOrDefault().ItemIndex;

        if (PrevPageNo < pageNo && pageNo <= MaxPageNo)
        {
            if (lastIndex < createCount)
            {
                for (int i = 0; i < count; i++)
                {
                    //  ����
                    for (int line = 0; line < LineColumn; line++)
                    {
                        var prevItem = ScrollItemList.FirstOrDefault();
                        var checkSize = (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);
                        checkSize.x += prevItem.ItemSize.x;
                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport, checkSize, Camera.main) ==
                            false)
                        {
                            scrollPool.PushItem(prevItem);
                            ScrollItemList.RemoveAt(0);
                        }
                    }

                    //  ����
                    for (int line = 0; line < LineColumn; line++)
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

                PrevPageNo = pageNo < 0
                    ? 0
                    : pageNo;
            }
        }

        if (0 <= pageNo && pageNo < PrevPageNo)
        {
            if (firstIndex > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var tempMax = LineColumn;
                    if (lastIndex == createCount - 1 && createCount % LineColumn != 0)
                    {
                        tempMax = createCount % LineColumn;
                    }

                    //  ����
                    for (int line = 0; line < tempMax; line++)
                    {
                        var prevItem = ScrollItemList.LastOrDefault();
                        var checkSize = (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);
                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport, checkSize, Camera.main) == false)
                        {
                            scrollPool.PushItem(prevItem);
                            ScrollItemList.RemoveAt(ScrollItemList.Count - 1);
                        }
                    }

                    //  ����
                    for (int line = 0; line < LineColumn; line++)
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

                PrevPageNo = pageNo >= MaxPageNo
                    ? MaxPageNo
                    : pageNo;
            }
        }
    }
}