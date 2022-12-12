using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class MenuTabView : MonoBehaviour {
    [Serializable]
    public struct Content {
        public Button tabBtn;
        public Sprite selectSprite;
        public Sprite unSelectSprite;
        public GameObject content;
    }

    public GameObject defaultTabBtn;
    private GameObject _currentTabBtn;
    public List<Content> contentList;

    public void Start() {
        if (contentList.Count == 0) {
            return;
        }

        foreach (var btn in contentList.Select(content => content.tabBtn)) {
            btn.onClick.AddListener(OnTabBtnClick);
        }

        _currentTabBtn = defaultTabBtn;
        _currentTabBtn.GetComponent<RectTransform>().SetAsLastSibling();
        foreach (var content in contentList) {
            content.content.SetActive(content.tabBtn.gameObject.Equals(defaultTabBtn));
        }
    }

    private void OnTabBtnClick() {
        var target = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (target.Equals(_currentTabBtn)) {
            return;
        }
        target.GetComponent<RectTransform>().SetAsLastSibling();
        _currentTabBtn.GetComponent<RectTransform>().SetAsFirstSibling();
        _currentTabBtn = target;

        foreach (var content in contentList) {
            if (content.tabBtn.gameObject.Equals(target)) {
                content.content.SetActive(true);
                if (content.selectSprite != null) {
                    content.tabBtn.GetComponent<Image>().sprite = content.selectSprite;
                }
            }
            else {
                content.content.SetActive(false);
                if (content.selectSprite != null) {
                    content.tabBtn.GetComponent<Image>().sprite = content.unSelectSprite;
                }
            }
        }
    }
}