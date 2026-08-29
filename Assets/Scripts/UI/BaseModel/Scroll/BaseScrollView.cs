using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace yjs.DevKit.UI
{
    [RequireComponent(typeof(ScrollRect))]
    [RequireComponent(typeof(BaseScrollPool))]
    public abstract class BaseScrollView : MonoBehaviour
    {
        public enum eScrollType
        {
            Left_To_Right,
            Right_To_Left,

            Top_To_Down,
            Bottom_To_Up,
        }

        [SerializeField] protected BaseScrollPool scrollPool;

        public eScrollType scrollType = eScrollType.Top_To_Down;

        private int ViewItemCount = 0;

        protected int LineRow = 1; //  가로
        protected int LineColumn = 1; //  세로

        protected List<BaseScrollData> ScrollDataList = new List<BaseScrollData>();
        protected List<BaseScrollItem> ScrollItemList = new List<BaseScrollItem>();
        protected BaseScrollItem ScrollItem => scrollPool.ItemPrefab;
        protected ScrollRect ScrollRect;
        protected int PrevPageNo = 0;
        protected int MaxPageNo = 0;


        public int createCount = 5;

        private void Awake()
        {
            if (ScrollRect == null)
            {
                ScrollRect = GetComponent<ScrollRect>();
                if (ScrollRect == null)
                    return;
            }

            SetScrollComponent();
        }

        protected void OnEnable()
        {
            ScrollRect?.onValueChanged.AddListener(OnValueChanged_Scroll);
        }

        protected void OnDisable()
        {
            ScrollRect?.onValueChanged.RemoveListener(OnValueChanged_Scroll);
        }

        public void InitScroll<T>(List<T> list) where T : BaseScrollData
        {
            ClearItems();
            ScrollDataList.Clear();
            ScrollDataList.AddRange(list);

            createCount = list.Count;

            switch (scrollType)
            {
                case eScrollType.Top_To_Down:
                case eScrollType.Bottom_To_Up:
                    {
                        var rect = ScrollRect.viewport.rect;
                        var scrollWidth = rect.size.x;
                        var scrollHeight = rect.size.y;

                        if (scrollType == eScrollType.Top_To_Down)
                        {
                            ScrollRect.content.anchorMin = new Vector2(0, 1);
                            ScrollRect.content.anchorMax = new Vector2(0, 1);
                            scrollPool.ItemPrefab.Rect.anchorMin = new Vector2(0, 1);
                            scrollPool.ItemPrefab.Rect.anchorMax = new Vector2(0, 1);
                        }
                        else
                        {
                            ScrollRect.content.anchorMin = new Vector2(0, 0);
                            ScrollRect.content.anchorMax = new Vector2(0, 0);
                            scrollPool.ItemPrefab.Rect.anchorMin = new Vector2(0, 0);
                            scrollPool.ItemPrefab.Rect.anchorMax = new Vector2(0, 0);
                        }

                        ViewItemCount = Mathf.RoundToInt(scrollHeight / ScrollItem.ItemSize.y);
                        LineRow = Mathf.FloorToInt(scrollWidth / scrollPool.ItemPrefab.ItemSize.x);

                        var contentWidth = LineRow * scrollPool.ItemPrefab.ItemSize.x;
                        var contentHeight = LineRow <= 1
                            ? ScrollItem.ItemSize.y * createCount
                            : ScrollItem.ItemSize.y * ((createCount / LineRow) + ((createCount) % LineRow == 0 ? 0 : 1));

                        MaxPageNo = Mathf.FloorToInt(contentHeight / scrollPool.ItemPrefab.ItemSize.y) - ViewItemCount;

                        //  크기
                        var contentRect = ScrollRect.content;
                        contentRect.sizeDelta = new Vector2(contentWidth, contentHeight);
                        ScrollRect.content = contentRect;

                        //  위치
                        var moveCenterX = (scrollWidth - contentWidth) * 0.5f;
                        var pos = ScrollRect.content.localPosition;
                        // pos.x += moveCenterX;

                        ScrollRect.content.localPosition = pos;
                        ScrollRect.verticalNormalizedPosition = scrollType == eScrollType.Top_To_Down
                            ? 1
                            : 0;

                        //  아이템 생성
                        int index = 0;
                        for (int i = 0; i <= ViewItemCount; i++)
                        {
                            for (int row = 0; row < LineRow; row++)
                            {
                                if (index >= createCount)
                                {
                                    break;
                                }

                                var no = index++;
                                var item = RepositionItem(ScrollDataList[no], no, scrollType);
                                item.SetActive(true);

                                ScrollItemList.Add(item);
                            }
                        }

                        //  초기 셋팅시 스크롤 페이지 번호 저장
                        PrevPageNo = scrollType == eScrollType.Top_To_Down
                            ? 0
                            : MaxPageNo;

                        break;
                    }

                case eScrollType.Left_To_Right:
                case eScrollType.Right_To_Left:
                    {
                        var rect = ScrollRect.viewport.rect;
                        var scrollWidth = rect.size.x;
                        var scrollHeight = rect.size.y;

                        if (scrollType == eScrollType.Right_To_Left)
                        {
                            ScrollRect.content.anchorMin = new Vector2(1, 1);
                            ScrollRect.content.anchorMax = new Vector2(1, 1);
                            ScrollRect.content.pivot = new Vector2(1, 1);
                            scrollPool.ItemPrefab.Rect.anchorMin = new Vector2(1, 1);
                            scrollPool.ItemPrefab.Rect.anchorMax = new Vector2(1, 1);
                            scrollPool.ItemPrefab.Rect.pivot = new Vector2(1, 1);
                        }
                        else
                        {
                            ScrollRect.content.anchorMin = new Vector2(0, 1);
                            ScrollRect.content.anchorMax = new Vector2(0, 1);
                            ScrollRect.content.pivot = new Vector2(0, 1);
                            scrollPool.ItemPrefab.Rect.anchorMin = new Vector2(0, 1);
                            scrollPool.ItemPrefab.Rect.anchorMax = new Vector2(0, 1);
                            scrollPool.ItemPrefab.Rect.pivot = new Vector2(0, 1);
                        }

                        ViewItemCount = Mathf.RoundToInt(scrollWidth / ScrollItem.ItemSize.x);
                        LineColumn = Mathf.FloorToInt(scrollHeight / scrollPool.ItemPrefab.ItemSize.y);

                        var contentWidth = LineColumn <= 1
                            ? ScrollItem.ItemSize.x * createCount
                            : ScrollItem.ItemSize.x *
                              ((createCount / LineColumn) + ((createCount) % LineColumn == 0 ? 0 : 1));
                        var contentHeight = LineColumn * scrollPool.ItemPrefab.ItemSize.y;

                        MaxPageNo = Mathf.FloorToInt(contentWidth / scrollPool.ItemPrefab.ItemSize.x) - ViewItemCount;

                        //  크기
                        var contentRect = ScrollRect.content;
                        contentRect.sizeDelta = new Vector2(contentWidth, contentHeight);
                        ScrollRect.content = contentRect;

                        //  위치
                        var moveCenterY = (scrollHeight - contentHeight) * 0.5f;
                        var pos = ScrollRect.content.localPosition;
                        pos.y -= moveCenterY;
                        ScrollRect.content.localPosition = pos;
                        ScrollRect.horizontalNormalizedPosition = scrollType == eScrollType.Right_To_Left ? 1 : 0;

                        //  아이템 생성
                        int index = 0;
                        for (int i = 0; i <= ViewItemCount; i++)
                        {
                            for (int column = 0; column < LineColumn; column++)
                            {
                                if (index >= createCount)
                                    break;

                                var no = index++;
                                var item = RepositionItem(ScrollDataList[no], no, scrollType);
                                item.SetActive(true);

                                ScrollItemList.Add(item);
                            }
                        }

                        //  초기 셋팅시 스크롤 페이지 번호 저장
                        PrevPageNo = 0;
                        break;
                    }
            }
        }

        public void ClearItems()
        {
            foreach (var item in ScrollItemList)
                scrollPool.PushItem(item);

            ScrollItemList.Clear();
        }

        /// <summary>
        /// 스크롤 설정 값에 맞게 스크롤 컴포넌트 셋팅 변경
        /// </summary>
        protected virtual void SetScrollComponent()
        {
            ScrollRect.vertical = false;
            ScrollRect.horizontal = false;

            if (scrollType == eScrollType.Top_To_Down ||
                scrollType == eScrollType.Bottom_To_Up)
            {
                ScrollRect.vertical = true;
            }
            else if (scrollType == eScrollType.Right_To_Left ||
                     scrollType == eScrollType.Left_To_Right)
            {
                ScrollRect.horizontal = true;
            }
        }

        /// <summary>
        /// 각 아이템 인덱스 별 위치조정
        /// </summary>
        /// <param name="item">아이템 오브젝트 </param>
        /// <param name="index">아이템 인덱스 </param>
        /// <param name="type">현재 아이템 스크롤 방향 </param>
        /// <returns></returns>
        protected virtual BaseScrollItem RepositionItem(BaseScrollData data, int index, eScrollType type)
        {
            var item = scrollPool.PopItem(ScrollRect.content, index);

            var posX = 0f;
            var posY = 0f;
            switch (scrollType)
            {
                case eScrollType.Top_To_Down:
                case eScrollType.Bottom_To_Up:
                    {
                        posX = index % LineRow * ScrollItem.ItemSize.x + (ScrollItem.ItemSize.x * 0.5f);

                        posY = scrollType == eScrollType.Top_To_Down
                            ? -(index / LineRow * ScrollItem.ItemSize.y) - (ScrollItem.ItemSize.y * 0.5f)
                            : (index / LineRow * ScrollItem.ItemSize.y) + ScrollItem.ItemSize.y -
                              (ScrollItem.ItemSize.y * 0.5f);
                        break;
                    }

                case eScrollType.Right_To_Left:
                case eScrollType.Left_To_Right:
                    {
                        posX = scrollType == eScrollType.Right_To_Left
                            ? -(index / LineColumn * ScrollItem.ItemSize.x)
                            : index / LineColumn * ScrollItem.ItemSize.x;

                        posY = -(index % LineColumn * ScrollItem.ItemSize.y);
                        break;
                    }
            }

            item.Rect.anchoredPosition = new Vector2(posX, posY);

            item.UpdateItem(data);

            return item;
        }

        /// <summary>
        /// 스크롤 업데이트
        /// </summary>
        /// <param name="pos">스크롤 Content 위치 변</param>
        protected virtual void OnValueChanged_Scroll(Vector2 pos)
        {
            var contentPos = GetScrollContentsPosition();

            switch (scrollType)
            {
                case eScrollType.Top_To_Down:
                    {
                        var pageNo = Mathf.FloorToInt(contentPos.y / ScrollItem.ItemSize.y);
                        var count = Mathf.Abs(pageNo - PrevPageNo);

                        if (pageNo >= PrevPageNo && pageNo <= MaxPageNo)
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
                                        var checkSize =
                                            (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);
                                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport,
                                                checkSize,
                                                Camera.main) == false)
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

                                PrevPageNo = pageNo > MaxPageNo
                                    ? MaxPageNo
                                    : pageNo;
                            }
                        }

                        if (pageNo < PrevPageNo && pageNo >= 0)
                        {
                            var firstIndex = (int)ScrollItemList.FirstOrDefault()?.ItemIndex;
                            if (firstIndex >= 0)
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    //  ����
                                    for (int line = 0; line < LineRow; line++)
                                    {
                                        var prevItem = ScrollItemList.LastOrDefault();
                                        var checkSize =
                                            (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);
                                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport,
                                                checkSize,
                                                Camera.main) == false)
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

                                PrevPageNo = pageNo >= 0
                                    ? pageNo
                                    : 0;
                            }
                        }

                        break;
                    }
                case eScrollType.Bottom_To_Up:
                    {
                        // var contentPos = GetScrollContentsPosition();
                        var pageNo = Mathf.FloorToInt(contentPos.y / ScrollItem.ItemSize.y) - LineRow;
                        var count = Mathf.Abs(pageNo - PrevPageNo);

                        //
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
                                        var checkSize =
                                            (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);

                                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport,
                                                checkSize,
                                                Camera.main) == false)
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
                        //  아래쪽으로 스크롤시
                        if (pageNo > PrevPageNo && pageNo <= MaxPageNo)
                        {
                            var firstIndex = (int)ScrollItemList.FirstOrDefault()?.ItemIndex;
                            if (firstIndex >= 0)
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    var tempMax = ScrollItemList.FirstOrDefault().ItemIndex == createCount - 1
                                        ? createCount % LineRow
                                        : LineRow;

                                    //  위쪽 라인 삭제
                                    for (int line = 0; line < tempMax; line++)
                                    {
                                        var prevItem = ScrollItemList.LastOrDefault();
                                        var checkSize =
                                            (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);
                                        checkSize.y -= prevItem.ItemSize.y;
                                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport,
                                                checkSize,
                                                Camera.main) == false)
                                        {
                                            scrollPool.PushItem(prevItem);
                                            ScrollItemList.RemoveAt(ScrollItemList.Count - 1);
                                        }
                                    }

                                    //  아래쪽 라인 추가
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

                        break;
                    }

                case eScrollType.Right_To_Left:
                    {
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
                                    //  가려짐
                                    for (int line = 0; line < LineColumn; line++)
                                    {
                                        var prevItem = ScrollItemList.FirstOrDefault();
                                        var checkSize =
                                            (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);
                                        checkSize.x += prevItem.ItemSize.x;
                                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport,
                                                checkSize,
                                                Camera.main) ==
                                            false)
                                        {
                                            scrollPool.PushItem(prevItem);
                                            ScrollItemList.RemoveAt(0);
                                        }
                                    }

                                    //  추가
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

                                    //  가려짐
                                    for (int line = 0; line < tempMax; line++)
                                    {
                                        var prevItem = ScrollItemList.LastOrDefault();
                                        var checkSize =
                                            (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);
                                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport,
                                                checkSize,
                                                Camera.main) == false)
                                        {
                                            scrollPool.PushItem(prevItem);
                                            ScrollItemList.RemoveAt(ScrollItemList.Count - 1);
                                        }
                                    }

                                    //  추가
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

                        break;
                    }

                case eScrollType.Left_To_Right:
                    {
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
                                    //  가려짐
                                    for (int line = 0; line < LineColumn; line++)
                                    {
                                        var prevItem = ScrollItemList.FirstOrDefault();
                                        var checkSize =
                                            (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);
                                        checkSize.x += prevItem.ItemSize.x;
                                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport,
                                                checkSize,
                                                Camera.main) ==
                                            false)
                                        {
                                            scrollPool.PushItem(prevItem);
                                            ScrollItemList.RemoveAt(0);
                                        }
                                    }

                                    //  추가
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

                                PrevPageNo = pageNo > MaxPageNo
                                    ? MaxPageNo
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

                                    //  가려짐
                                    for (int line = 0; line < tempMax; line++)
                                    {
                                        var prevItem = ScrollItemList.LastOrDefault();
                                        var checkSize =
                                            (Vector2)Camera.main.WorldToScreenPoint(prevItem.transform.position);
                                        checkSize.x += prevItem.ItemSize.x;
                                        if (RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport,
                                                checkSize,
                                                Camera.main) == false)
                                        {
                                            scrollPool.PushItem(prevItem);
                                            ScrollItemList.RemoveAt(ScrollItemList.Count - 1);
                                        }
                                    }

                                    //  추가
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

                                PrevPageNo = pageNo >= 0
                                    ? pageNo
                                    : 0;
                            }
                        }

                        break;
                    }
            }
        }

        protected Vector2 GetScrollContentsPosition()
        {
            var rect = (RectTransform)ScrollRect.transform;
            var contentPos = ScrollRect.content.anchoredPosition;

            if (scrollType == eScrollType.Left_To_Right)
            {
                var maxWidth = (ScrollRect.content.sizeDelta.x - rect.sizeDelta.x);
                var pos = contentPos.x * -1;
                if (pos < 0)
                    return new Vector2(0, 0);
                if (pos > maxWidth)
                    return new Vector2(maxWidth, contentPos.y);

                return new Vector2(pos, contentPos.y);
            }
            else if (scrollType == eScrollType.Right_To_Left)
            {
                var maxWidth = (ScrollRect.content.sizeDelta.x - rect.sizeDelta.x);
                if (contentPos.x < 0)
                    return new Vector2(0, 0);
                if (contentPos.x > maxWidth)
                    return new Vector2(maxWidth, contentPos.y);

                return contentPos;
            }
            else if (scrollType == eScrollType.Bottom_To_Up)
            {
                var minHeigth = rect.sizeDelta.y;
                var maxHeight = ScrollRect.content.sizeDelta.y;//(ScrollRect.content.sizeDelta.y - rect.sizeDelta.y);
                if (contentPos.y < 0)
                    return new Vector2(0, minHeigth);
                if (contentPos.y > maxHeight)
                    return new Vector2(contentPos.x, maxHeight);

                return contentPos;
            }
            else
            {
                var maxHeight = (ScrollRect.content.sizeDelta.y - rect.sizeDelta.y);
                if (contentPos.y < 0)
                    return new Vector2(0, 0);
                if (contentPos.y > maxHeight)
                    return new Vector2(contentPos.x, maxHeight);

                return contentPos;
            }
        }
    }
}