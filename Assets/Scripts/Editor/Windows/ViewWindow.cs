using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using World_Model;


public class ViewWindow : EditorWindow
{ 




    
    #region Instancing
    private static ViewWindow _instance;
    public static ViewWindow Instance
    {
        get
        {
            if (_instance == null)
                _instance = GetWindow<ViewWindow>("Database");
            return _instance;
        }
    }
    private void VerifyInstance()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Close();
    } 
    [MenuItem("OnlyWorlds/View Window")]
    public static void ShowWindow()
    {
        Instance.Show();
    }
    #endregion
    #region WindowFunctions
    
 
    private void OnEnable()
    {
        VerifyInstance();
        RefreshSubscribes(true); 
        BuildWindow();
    } 

    private void RefreshSubscribes(bool subscribing)
    {
        if (subscribing)
        {
            RootControl.OnWorldChanged += RefreshWorld;
            RootControl.OnWindowRefresh += RefreshBlank;
            RootControl.OnTableChanged += RefreshTable;
            RootControl.OnElementChanged += RefreshElement;
            RootControl.OnElementEdited += RefreshElement;
         //   RootControl.OnFieldChanged += RefreshField;
            RootControl.OnPinChanged += RefreshPin;
            RootControl.OnMapChanged += RefreshMap;
            RootControl.OnTimestateChanged += RefreshTimestate;
        }
        else
        {
            RootControl.OnWorldChanged -= RefreshWorld;
            RootControl.OnWindowRefresh -= RefreshBlank;
            RootControl.OnTableChanged -= RefreshTable;
            RootControl.OnElementChanged -= RefreshElement;
            RootControl.OnElementEdited -= RefreshElement;
          //  RootControl.OnFieldChanged -= RefreshField;
            RootControl.OnPinChanged -= RefreshPin;
            RootControl.OnMapChanged -= RefreshMap;
            RootControl.OnTimestateChanged -= RefreshTimestate;
        }
    }

  

    private void OnGUI()
    {
        /*
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            GUI.FocusControl(null);  
            Repaint();
        }
        */

    }
    private void OnFocus()
    {
        
    } 
    private void OnLostFocus()
    {
        
    }

    private void OnDisable()
    {
        RefreshSubscribes(false); 

    }

    private void OnDestroy()
    {
        RefreshSubscribes(false); 

    }
    #endregion
    #region RefreshFunctions
    
    private void RefreshBlank()
    {
        BuildWindow();
    }  
    private void RefreshWorld(World newWorld)
    {
        if (newWorld == null)
            return;
        BuildWindow();
        // Set location active 
        CategorySelectors.GetCategorySelectors()[2].Activate();
        RefreshResults();
        RebuildCustomization();
    }
    private void RefreshTable(Element.Table newTable)
    { 
        BuildWindow();
    }
    private void RefreshElement(Element newElement)
    {
        if (newElement == null)
        {
            RefreshLossElement();
            return;
        }
        RefreshResults(); 

    } 
    private void RefreshField(Field newField)
    {
        if (newField == null)
        {
            RefreshLossField();
            return;
        }
      
    }
    private void RefreshMap(Map newMap)
    {
        if (newMap == null)
        {
            RefreshLossMap();
            return;
        }
    }
    private void RefreshTimestate(Timestate newTimestate)
    {
        if (newTimestate == null)
        {
            RefreshLossTimestate();
            return;
        }
    }
    private void RefreshPin(Pin newPin)
    {
        if (newPin == null)
        {
            RefreshLossPin();
            return;
        }
        RefreshResults();   
    }

    private void RefreshLossPin()
    {
         
    }

    private void RefreshLossElement()
    { 
        RefreshResults();
    }
    private void RefreshLossField ()
    { 
        
    }   
    private void RefreshLossMap ()
    { 
        
    }  
    private void RefreshLossTimestate ()
    { 
        
    }
    #endregion
    #region UI Building

    private VisualElement windowContent;
    
    
    private VisualElement BrowseRoot;
    private VisualElement SelectorsRoot;
    private  SelectorRow TopSelectors;
    private SelectorRow CategorySelectors;
    private SelectorRow ElementSelectorsOne;
    private SelectorRow ElementSelectorsTwo;
    private SelectorRow ElementSelectorsThree;
    private SelectorRow ElementSelectorsFour;
    private SelectorRow ElementSelectorsFive; 
       
    private VisualElement ResultsRoot;
    private VisualElement ExamineRoot;
    private VisualElement ExamineContent;
    private SearchBox searchInputBox;
    private SlideBar filterSlideBar;
    private VisualElement resultsContainer;
    private VisualElement browseContainer;
    private VisualElement customizeContainer;
    private VisualElement customizeContent;
    
    
    

       private void BuildStructure()
       {
           BuildTopSelectors();
           BuildCategoryAndElementSelectors(); 
           BuildResultsRoot();
           BuildCustomizeRoot();
       }

       
       // called on onenable, world load
    private void BuildWindow()
    { 
        rootVisualElement.Clear(); 
        rootVisualElement.style.backgroundColor = RootView.backgroundColor;

        windowContent = new VisualElement();
        BrowseRoot = new VisualElement();
        ExamineRoot = new VisualElement(); 
        ResultsRoot = new VisualElement();
        browseContainer = new VisualElement(); 
        customizeContainer = new VisualElement();
        customizeContent = new VisualElement();
        browseContainer.name = "browseContainer";
        customizeContainer.name = "customizeContainer";
        customizeContent.name = "customizeContent";
        BuildStructure();
        ResetStructureState();

        windowContent.style.marginTop = RootView.ContentMargin;
        windowContent.style.marginLeft = RootView.ContentMargin;
        windowContent.style.marginRight = RootView.ContentMargin;
        windowContent.style.marginBottom = RootView.ContentMargin;
        windowContent.Add(TopSelectors);
        windowContent.Add(BrowseRoot);
        windowContent.Add(ExamineRoot);
        windowContent.Add(browseContainer); 
        windowContent.Add(customizeContainer);
        rootVisualElement.Add(windowContent);
        Repaint();
        UpdateViewWindowCallbacks();
    }

    private void ResetStructureState()
    {
        ExamineRoot.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        
        foreach (var topSelector in TopSelectors.GetTopSelectors())
            topSelector.SetEnabled(true);
        TopSelectors.GetTopSelectors()[0].Activate();
        TopSelectors.GetTopSelectors()[1].Deactivate(); 
      
      //  if (RootControl.Element == null)
     //       TopSelectors.GetTopSelectors()[1].Disable();
        
    }


    private EventCallback<KeyDownEvent> _keyPressCallback;
    private void UpdateViewWindowCallbacks()
    {
        if (_keyPressCallback != null)
            rootVisualElement.UnregisterCallback(_keyPressCallback);
        
        rootVisualElement.focusable = true;
        _keyPressCallback = (e) => PressKey(e);
        rootVisualElement.RegisterCallback(_keyPressCallback, TrickleDown.TrickleDown);
    }
    private void PressKey(KeyDownEvent evt)
    { 
        if (evt.keyCode == KeyCode.Escape)
        {
            if (RootControl.Element != null)
                RootControl.SetElement(null);
        }
    }
    
    #region Selectors
       private TopSelector browseSelector;
       private TopSelector examineSelector;
       private void BuildTopSelectors()
       {
           TopSelectors = new SelectorRow(RootView.TopSelectorsHeight);

           browseSelector = new TopSelector("Browse", RootView.GetSelectorSprite("Browse"), ClickBrowseLeft, ClickBrowseRight, EnterBrowseWindow, LeaveBrowseWindow, MiddleClickBrowseSelector);
           examineSelector = new TopSelector("Customize", RootView.GetSelectorSprite("Examine"), ClickExamineLeft, ClickExamineRight,  EnterExamineWindow, LeaveExamineWindow, MiddleClickExamineSelector);

           TopSelectors.Add(browseSelector);
           TopSelectors.Add(examineSelector);
       } 

       private void ClickBrowseLeft()
       {
           foreach (var selector in TopSelectors.GetSelectors())
               selector.Deactivate();
           TopSelectors.GetSelectors()[0].Activate();
           browseContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
           customizeContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
       }
       private void ClickBrowseRight()
       {
           // if ALL are active, deactivate all 
           // otherwise activate all category&element containers
           UpdateSelectorsList();
           bool allActive = true;
           foreach (var selector in selectorsList)
               if (selector.Activated == false)
                   allActive = false;

           if (allActive)
               foreach (var selector in selectorsList)
                   selector.Deactivate();
           else
               foreach (var selector in selectorsList)
                   selector.Activate();
           RefreshResults();
       }
       private void EnterBrowseWindow()
       {
           TopSelectors.GetTopSelectors()[0].Hover();

       }
       private void LeaveBrowseWindow()
       {
           TopSelectors.GetTopSelectors()[0].ExitHover();
       }
      
       private void ClickExamineLeft()
       {
           foreach (var selector in TopSelectors.GetSelectors())
               selector.Deactivate();
           TopSelectors.GetSelectors()[1].Activate();
           
           browseContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
          customizeContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
          RebuildCustomization();

       }

       private void ClickExamineRight()
       {
            
       }
       private void EnterExamineWindow()
       {
           TopSelectors.GetTopSelectors()[1].Hover();

       }
       private void LeaveExamineWindow()
       {
           TopSelectors.GetTopSelectors()[1].ExitHover();
       }
       
       private void MiddleClickBrowseSelector()
       {
           // if ONE OR MORE Rows are hidden, show all
           // otherwise hide all
           bool anyRowsAreHidden = false || CategorySelectors.Hidden || ElementSelectorsOne.Hidden || ElementSelectorsTwo.Hidden || ElementSelectorsThree.Hidden || ElementSelectorsFour.Hidden || ElementSelectorsFive.Hidden;

           if (anyRowsAreHidden)
           {
               CategorySelectors.Unhide();
               ElementSelectorsOne.Unhide(); 
               ElementSelectorsTwo.Unhide();
               ElementSelectorsThree.Unhide();
               ElementSelectorsFour.Unhide();
               ElementSelectorsFive.Unhide();
           }
           else
           {
               CategorySelectors.Hide(false);
               ElementSelectorsOne.Hide(false); 
               ElementSelectorsTwo.Hide(false);
               ElementSelectorsThree.Hide(false);
               ElementSelectorsFour.Hide(false);
               ElementSelectorsFive.Hide(false);
           }
           RefreshCategoryElementSelectors();
           RefreshAfterElementSelectorChange(); 
       }
       private void MiddleClickExamineSelector()
       {
       

       }
       private void MiddleClickCategorySelector(CategorySelector selector)
       {
           selector.ParentRow.Hide(true);
           RefreshCategoryElementSelectors();
           RefreshAfterElementSelectorChange(); 
       }
       private void MiddleClickElementSelector(ElementSelector selector)
       {
           selector.ParentRow.Hide(true);
           RefreshCategoryElementSelectors();
           RefreshAfterElementSelectorChange(); 
       }
       private void LeftClickCategorySelector(CategorySelector selector)
       { 
           if (selector.Activated)
               isLeftMouseDeactivating = true;
           else
               isLeftMouseDeactivating = false;
           isLeftMouseDown = true;
           if (isLeftMouseDeactivating)
               selector.Deactivate();
           else
               selector.Activate();
           RefreshAfterElementSelectorChange();
       }

       private void RefreshAfterElementSelectorChange()
       {
           RefreshSortBar(); 
           RefreshResults();  
           RebuildCustomization();

       }
       private void BuildCategoryAndElementSelectors()
       {
           // create four rows of three, only style char/obj/loc in diff scheme
           CategorySelectors = new SelectorRow(RootView.CategorySelectorsHeight); 
           ElementSelectorsOne = new SelectorRow(RootView.ElementSelectorHeight); 
           ElementSelectorsTwo = new SelectorRow(RootView.ElementSelectorHeight); 
           ElementSelectorsThree = new SelectorRow(RootView.ElementSelectorHeight); 
           ElementSelectorsFour = new SelectorRow(RootView.ElementSelectorHeight); 
           ElementSelectorsFive = new SelectorRow(RootView.ElementSelectorHeight); 

           CategorySelectors.Add (   new CategorySelector (1,Element.Table.Character.ToString(), RootView.GetSelectorSprite(Element.Table.Character.ToString()), LeftClickCategorySelector, RightClickCategorySelector, HoverEnterCategorySelector, HoverExitCategorySelector, MiddleClickCategorySelector, LeftMouseKeyUp));
           CategorySelectors.Add (   new CategorySelector (2,Element.Table.Object.ToString(), RootView.GetSelectorSprite(Element.Table.Object.ToString()), LeftClickCategorySelector, RightClickCategorySelector, HoverEnterCategorySelector, HoverExitCategorySelector, MiddleClickCategorySelector, LeftMouseKeyUp));
           CategorySelectors.Add (   new CategorySelector (3,Element.Table.Location.ToString(), RootView.GetSelectorSprite(Element.Table.Location.ToString()), LeftClickCategorySelector, RightClickCategorySelector, HoverEnterCategorySelector, HoverExitCategorySelector, MiddleClickCategorySelector, LeftMouseKeyUp));
           ElementSelectorsOne.Add (   new ElementSelector(Element.Table.Family.ToString(), RootView.GetSelectorSprite(Element.Table.Family.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsOne.Add (   new ElementSelector(Element.Table.Territory.ToString(), RootView.GetSelectorSprite(Element.Table.Territory.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsOne.Add (   new ElementSelector(Element.Table.Institution.ToString(), RootView.GetSelectorSprite(Element.Table.Institution.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsTwo.Add (   new ElementSelector(Element.Table.Race.ToString(), RootView.GetSelectorSprite(Element.Table.Race.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsTwo.Add (   new ElementSelector(Element.Table.Creature.ToString(), RootView.GetSelectorSprite(Element.Table.Creature.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsTwo.Add (   new ElementSelector(Element.Table.Collective.ToString(), RootView.GetSelectorSprite(Element.Table.Collective.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsThree.Add (   new ElementSelector(Element.Table.Trait.ToString(), RootView.GetSelectorSprite(Element.Table.Trait.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsThree.Add (   new ElementSelector(Element.Table.Force.ToString(), RootView.GetSelectorSprite(Element.Table.Force.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsThree.Add (   new ElementSelector(Element.Table.Title.ToString(), RootView.GetSelectorSprite(Element.Table.Title.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsFour.Add (   new ElementSelector(Element.Table.Ability.ToString(), RootView.GetSelectorSprite(Element.Table.Event.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsFour.Add (   new ElementSelector(Element.Table.Language.ToString(), RootView.GetSelectorSprite(Element.Table.Language.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
             ElementSelectorsFour.Add (   new ElementSelector(Element.Table.Law.ToString(), RootView.GetSelectorSprite(Element.Table.Law.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsFive.Add (   new ElementSelector(Element.Table.Relation.ToString(), RootView.GetSelectorSprite(Element.Table.Relation.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));
           ElementSelectorsFive.Add (   new ElementSelector(Element.Table.Event.ToString(), RootView.GetSelectorSprite(Element.Table.Event.ToString()), LeftClickElementSelector, RightClickElementSelector, HoverEnterElementSelector, HoverExitElementSelector, MiddleClickElementSelector, LeftMouseKeyUp));

           if (SelectorsRoot == null)
               SelectorsRoot = new VisualElement();
           SelectorsRoot.Clear();
            RefreshCategoryElementSelectors();
           
           SelectorsRoot.Add(CategorySelectors);
           SelectorsRoot.Add(ElementSelectorsOne);
           SelectorsRoot.Add(ElementSelectorsTwo);
           SelectorsRoot.Add(ElementSelectorsThree);
           SelectorsRoot.Add(ElementSelectorsFour);
           SelectorsRoot.Add(ElementSelectorsFive);  
           BrowseRoot.Add(SelectorsRoot);
      
        DeactivateCategoryAndElementSelectors();
       }

       private void RefreshCategoryElementSelectors()
       {
           TopSelectors.style.minHeight = RootView.TopSelectorsHeight;
           float totalHeight = 0f;
           if (CategorySelectors.Hidden == false)
               totalHeight += RootView.CategorySelectorsHeight;
           if (ElementSelectorsOne.Hidden == false)
               totalHeight += RootView.ElementSelectorHeight;
           if (ElementSelectorsTwo.Hidden == false)
               totalHeight += RootView.ElementSelectorHeight;
           if (ElementSelectorsThree.Hidden == false)
               totalHeight += RootView.ElementSelectorHeight;
           if (ElementSelectorsFour.Hidden == false)
               totalHeight += RootView.ElementSelectorHeight;
           if (ElementSelectorsFive.Hidden == false)
               totalHeight += RootView.ElementSelectorHeight;
           
           SelectorsRoot.style.minHeight = totalHeight; 
       }
       
       
       private List<Selector> selectorsList;

       private void UpdateSelectorsList()
       {
               selectorsList = CategorySelectors.GetCategorySelectors().Cast<Selector>().ToList();
               selectorsList.AddRange(ElementSelectorsOne.GetElementSelectors().Cast<Selector>().ToList());
               selectorsList.AddRange(ElementSelectorsTwo.GetElementSelectors().Cast<Selector>().ToList());
               selectorsList.AddRange(ElementSelectorsThree.GetElementSelectors().Cast<Selector>().ToList()); 
               selectorsList.AddRange(ElementSelectorsFour.GetElementSelectors().Cast<Selector>().ToList()); 
               selectorsList.AddRange(ElementSelectorsFive.GetElementSelectors().Cast<Selector>().ToList()); 
       
       }
       
       private bool isLeftMouseDeactivating = false;
       private bool isLeftMouseDown = false;

       private void RightClickCategorySelector(CategorySelector selector )
       {
        // if category is only active out of all selectors, select its row 
 
        bool AllElementsAreInactive = false; 
        bool AllChildElementsAreActive = false;
        List<ElementSelector> childSelectors = new List<ElementSelector>(); 
        childSelectors.Add(ElementSelectorsOne.GetElementSelectors()[selector.selectorID-1]);
        childSelectors.Add(ElementSelectorsTwo.GetElementSelectors()[selector.selectorID-1]);
        childSelectors.Add(ElementSelectorsThree.GetElementSelectors()[selector.selectorID-1]);
        childSelectors.Add(ElementSelectorsFour.GetElementSelectors()[selector.selectorID-1]);
        if (selector.Activated)
        {
            bool somethingActive = false;
             AllChildElementsAreActive = true; 
            foreach (var childSelector in childSelectors)
                if (childSelector.Activated == false)
                    AllChildElementsAreActive = false;
            if (selector.Activated == false)
                AllChildElementsAreActive = false;


            UpdateSelectorsList(); 
    
            foreach (var givenSelector in selectorsList)
               if (givenSelector != selector)
                    if (givenSelector.Activated)
                        somethingActive = true;
            if (somethingActive == false)
                AllElementsAreInactive = true;
        }

        // set everything inactive
        DeactivateCategoryAndElementSelectors();

         if (AllChildElementsAreActive)
           { 
               selector.Activate();
               foreach (var elementSelector in childSelectors)
                   elementSelector.Deactivate();
           }
         else    if (AllElementsAreInactive) // deactivate when all four in the column are active
           { 
                
               foreach (var elementSelector in childSelectors)
                   elementSelector.Activate();
               selector.Activate();
           }
         else
         {
             selector.Activate();
         } 
         RefreshAfterElementSelectorChange();
       }
       private void HoverEnterCategorySelector(CategorySelector selector )
       { 
           if (isLeftMouseDown)
           {
               if (isLeftMouseDeactivating)
                   selector.Deactivate();
               else
                   selector.Activate();
           }
           else 
               selector.Hover();
       }
       private void HoverExitCategorySelector(CategorySelector selector )
       {
           selector.ExitHover();

       } 
       private void LeftClickElementSelector(ElementSelector selector)
       { 
           if (selector.Activated)
               isLeftMouseDeactivating = true;
           else
               isLeftMouseDeactivating = false;
           isLeftMouseDown = true;
           if (selector.Activated)
               selector.Deactivate();
           else
               selector.Activate();
           RefreshAfterElementSelectorChange();

       }
       private void RightClickElementSelector(ElementSelector selector )
       { 
           DeactivateCategoryAndElementSelectors();
           if (selector.Activated)
               selector.Deactivate();
           else
               selector.Activate();
           RefreshAfterElementSelectorChange();

       }
       private void HoverEnterElementSelector(ElementSelector selector )
       {
           if (isLeftMouseDown)
           {
               if (isLeftMouseDeactivating)
                   selector.Deactivate();
               else
                   selector.Activate();
           }
           else 
               selector.Hover();
       }
       private void HoverExitElementSelector(ElementSelector selector )
       {
           selector.ExitHover(); 
       }
        
       private void LeftMouseKeyUp()
       { 
           isLeftMouseDown = false;
       }
       private void DeactivateCategoryAndElementSelectors()
       {
           foreach (var categorySelector in GetCategorySelectors())
               categorySelector.Deactivate(); 
           foreach (var elementSelector in GetElementSelectors())
               elementSelector.Deactivate(); 
       }
       private List<CategorySelector> GetCategorySelectors()
       {
           List<CategorySelector> returnList = new List<CategorySelector>();
           foreach (var selector in CategorySelectors.GetCategorySelectors())
               returnList.Add(selector); 
           return returnList;
       }
       private List<ElementSelector> GetElementSelectors()
       {
           List<ElementSelector> returnList = new List<ElementSelector>();
           foreach (var selector in ElementSelectorsOne.GetElementSelectors())
               returnList.Add(selector);
           foreach (var selector in ElementSelectorsTwo.GetElementSelectors())
               returnList.Add(selector);
           foreach (var selector in ElementSelectorsThree.GetElementSelectors())
               returnList.Add(selector); 
           foreach (var selector in ElementSelectorsFour.GetElementSelectors())
               returnList.Add(selector);    
           foreach (var selector in ElementSelectorsFive.GetElementSelectors())
               returnList.Add(selector); 
           return returnList;
       }
       #endregion

       
       #region Search and Results

     
       private void BuildResultsRoot()
       {
           ResultsRoot.style.marginTop = 1f;

           resultsContainer = new VisualElement();
           resultsContainer.style.marginTop = -1f;
           resultsContainer.style.backgroundColor = RootControl.ResultsColorContainer;
           resultsContainer.AddToClassList("backgroundContainer");
           var stylesheet =   AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.staticProjectPath +"UI/USS/SelectorStyleSheet.uss"); 
           if (stylesheet == null)
               Debug.Log("!Stylesheet Null");
           resultsContainer.styleSheets.Add(stylesheet); 
           resultsContainer.style.borderBottomLeftRadius = 0f;
           resultsContainer.style.borderBottomRightRadius = 0f;
           resultsContainer.style.borderTopWidth = 0f;
           resultsContainer.style.borderBottomWidth = 0f;
           resultsContainer.style.borderRightWidth = 0f;
           resultsContainer.style.borderLeftWidth = 0f;  

           
           searchInputBox = new SearchBox(RootControl,"Search", OnTextChanged, OnSearchEnterPress);
           searchInputBox.tooltip = "Examples:" +
                                    "\n" + RootControl.Protagonist +
                                    "\nAge:>44 Location:" + RootControl.Homebase; 
           searchInputBox.style.borderTopLeftRadius = 8f;
           searchInputBox.style.borderTopRightRadius = 8f;
           searchInputBox.style.marginTop = 1f;
           searchInputBox.style.borderBottomWidth = 0f;
           searchInputBox.style.width = Length.Percent(100);
           searchText = "";

     

          
           var sortSlideBar =  BuildSortBar( );
               
       
           
           
           resultsContainer.Add(sortSlideBar);
           ResultsRoot.Add(searchInputBox);  
           ResultsRoot.Add(resultsContainer);
           BrowseRoot.Add(ResultsRoot); 
           browseContainer.Add(ResultsRoot); 
           RefreshResults();
       }

      
       Selector singleSelectedSelector = null; 

       private SquareDropdown fieldDropdown;
       private SquareDropdown orderDropdown; 
       private Element.Table  singleTable;
       private SlideBar BuildSortBar()
       {
           var leftRowOne = new SimpleRow();
           var leftRowTwo = new SimpleRow();
           var rightRowOne = new SimpleRow();
           var rightRowTwo = new SimpleRow();
           var sortSlideBar = new  SlideBar(RootView,  true, RootControl.ResultsColorFilterLeft, RootControl.ResultsColorFilterRight, leftRowOne,leftRowTwo, rightRowOne, rightRowTwo);
         sortSlideBar.style.minHeight = RootView.sortBarHeight;
         sortSlideBar.style.borderTopLeftRadius = 1f;
         sortSlideBar.style.borderTopRightRadius = 1f;
         sortSlideBar.style.borderBottomLeftRadius = 1f;
         sortSlideBar.style.borderBottomRightRadius = 1f;
         sortSlideBar.style.borderLeftWidth = 1f;
         sortSlideBar.style.borderRightWidth = 1f;
         sortSlideBar.style.borderBottomWidth = 0f;
         sortSlideBar.style.borderRightColor = Color.grey;
         sortSlideBar.style.borderLeftColor = Color.grey;
         sortSlideBar.style.borderBottomColor = Color.grey; 

            fieldDropdown = new SquareDropdown( RootControl,"fieldDropdown",  OnFieldDropdownChange);
            orderDropdown = new SquareDropdown(  RootControl,"orderDropdown",  OnOrderDropdownChange);
        
            leftRowOne.style.paddingTop = 3;
            leftRowOne.style.justifyContent = new StyleEnum<Justify>(Justify.FlexStart);
            leftRowOne.style.alignContent = new StyleEnum<Align>(Align.Center);

            RefreshSortBar();
           
            leftRowOne.Add(fieldDropdown);
            leftRowOne.Add(orderDropdown);
            
            fieldDropdown.style.width = Length.Percent(47);
            orderDropdown.style.width = Length.Percent(47); 
         return sortSlideBar;
       }

       private void OnFieldDropdownChange(string newValue)
       {
          RefreshResults();
       }   
       private void OnOrderDropdownChange(string newValue)
       {
           RefreshResults(); 
       }

       private void RefreshSortBar()
       {
           List<string> propertiesList = new List<string>(); 

           bool oneTableSelected = SingleTableSelected();

           if (!oneTableSelected)
           {
               propertiesList.Add("Name");
               propertiesList.Add("Description");
               propertiesList.Add("Date");
           }
           else
           {
               if(Element.tableTypes.TryGetValue(singleSelectedSelector.name, out Type tableType))
               {
                   var elementProperties = typeof(Element).GetProperties().Select(p => p.Name).ToList();

                   propertiesList = tableType.GetProperties()
                       .Where(p => (p.PropertyType.IsValueType || p.PropertyType == typeof(string)) && !elementProperties.Contains(p.Name))
                       .Select(p => p.Name)
                       .ToList();

                   if (propertiesList.Count == 0)
                   {
                       propertiesList.Add("Name");
                       propertiesList.Add("Description");
                       propertiesList.Add("Date");
                       propertiesList.Add("Pin");
                   } 
               } 
               else  
               {
                   // This should never happen
                   propertiesList.Add("Name");
                   propertiesList.Add("Description");
                   propertiesList.Add("Date");
                   propertiesList.Add("Pin");
               }
           }

           var  ordersList = new List<string>
           {
               "Ascend",
               "Descend",
               "Include",
               "Exclude"
           };
           orderDropdown.choices = ordersList;
           orderDropdown.value = ordersList[0];
           
           if (propertiesList  != null)
               if (propertiesList.Count != 0)
               { 
                   fieldDropdown.choices = propertiesList;
                   fieldDropdown.value = propertiesList[0];
               }  
       }
 
       #endregion

 
       
     
       private void CreateCustomizeBase()
       {
           customizeContainer.style.width = Length.Percent(100); 
           customizeContainer.style.minHeight = 200;
           customizeContainer.AddToClassList("backgroundContainer");
           var stylesheet =   AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.staticProjectPath +"UI/USS/SelectorStyleSheet.uss");  
           customizeContainer.styleSheets.Add(stylesheet); 
           customizeContainer.style.marginTop = 2f;
           //   customizeContainer.style.backgroundColor = Color.gray;
           customizeContainer.style.borderTopLeftRadius = 8f;
           customizeContainer.style.borderTopRightRadius = 8f;
           customizeContainer.style.borderTopColor = Color.grey;
           customizeContainer.style.borderLeftColor = Color.grey;
           customizeContainer.style.borderRightColor = Color.grey;
           customizeContainer.style.borderBottomColor = Color.grey;  
       }
       private void CreateCustomizeInvalidSelection()
       {
           Label invalidLabel = new Label("Select one individual Element Type to allow customization");
           invalidLabel.style.color = Color.white;
           invalidLabel.style.marginTop = 3f;
           invalidLabel.style.marginLeft = 1f;
           customizeContent.Add(invalidLabel);
       }
       /// <summary>
       /// Clear and create customizeContainer based on selected element Table. Empty if multiple or none selected
       /// </summary>
       private void RebuildCustomization()
       {
           customizeContainer.Clear();
           if (customizeContent != null)
               customizeContent.Clear();
           customizeContent = new VisualElement();
           customizeContent.style.width = Length.Percent(100);
           customizeContent.style.height = Length.Percent(100);
           customizeContainer.Add(customizeContent);
           bool oneTableSelected = SingleTableSelected();
           List<string> propertiesList = new List<string>();

           CreateCustomizeBase();

          
           customizeContent.style.backgroundColor = RootView.customizeContentColor; 
           customizeContent.style.marginLeft = 2f;
           
           
           if (oneTableSelected)
           {
               Element.Table table = (Element.Table)Enum.Parse(typeof(Element.Table), singleSelectedSelector.name);
               
               if (singleSelectedSelector.name == Element.Table.Character.ToString())
                   CreateCustomizeContentForCharacterTyping();
               else    if (singleSelectedSelector.name == Element.Table.Object.ToString())
                   CreateCustomizeContentForMatterTyping(); 
               else    
                   CreateCustomizeContentForDefaultTyping(table); 
           }
           else
           {
               CreateCustomizeInvalidSelection();
           } 
       }

       
      

       private List<EditField> typeFields;
       private List<EditField> typeCustomFields;
       private List<EditField> subtypeFields;
       private List<EditField> subtypeCustomFields;
    
      
       
       private List<TableTyping> typingTableList;

       private void CreateCustomizeContentForDefaultTyping(Element.Table table)
       {
           typingTableList = RootControl.WorldParser.GetTypingTablesForElementTable(table).ToList();

           CreateHeaderLabels();
           CreateEditFields();
       //    // read supertypes and customtypes from db 
       //     // same for subtypes and custom subtypes
       //    // put into editfields
       //     // detect click on supertype fields, to highlight it and update subtypes
       //    // add input listeners to editfields
           // write back to db 
           // allow adding subtypes
       }

       // Four columns each with a list of editfields
       private void CreateEditFields( )
       {
           VisualElement columnsRow = new VisualElement();
           columnsRow.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
           columnsRow.style.width = Length.Percent(100);
           columnsRow.style.height = Length.Percent(100);
           columnsRow.style.marginTop = 6;
           
           VisualElement columnOne = CreateEditfieldColumn(24);
           VisualElement columnTwo  = CreateEditfieldColumn(24);
           VisualElement columnThree  = CreateEditfieldColumn(4);
           VisualElement columnFour  = CreateEditfieldColumn(24);
           VisualElement columnFive  = CreateEditfieldColumn(24);

           ClearEditFields();

           
           float marginTop = 4f;
            // typeFields
           for (int i = 0; i < typingTableList.Count; i++)
           {
               EditField editField = new EditField(RootControl, false, "",typingTableList[i].Supertype  , 
                   columnOne.resolvedStyle.width  , null, null, null);
               editField.SetInteractable(false);
               editField.style.width = Length.Percent(85);
               editField.style.height = RootControl.EditFieldHeight;
               editField.style.marginTop = marginTop;
               typeFields.Add(editField);
               columnOne.Add(editField);
           } 
           // typeCustomFields 
           for (int i = 0; i < typingTableList.Count; i++)
           {
               string customString = typingTableList[i].Supertype;
               if (string.IsNullOrEmpty(typingTableList[i].TypeCustom) == false)
                   customString = typingTableList[i].TypeCustom;
               EditField editField = new EditField(RootControl, false, "",customString , 
                   columnOne.resolvedStyle.width , null, ClickEnterCustomType, null);
               editField.style.width = Length.Percent(85);
               editField.style.height = RootControl.EditFieldHeight;
               editField.style.marginTop = marginTop;
               typeCustomFields.Add(editField); 
               columnTwo.Add(editField);
           } 
           // subtypeFields
           for (int i = 0; i < typingTableList[clickedTypeFieldIndex].Subtypes.Count; i++)
           {
               EditField editField = new EditField(RootControl, false, "",typingTableList[clickedTypeFieldIndex].GetOriginalSubtypes()[i], 
                   columnOne.resolvedStyle.width  , null, null, null);
               editField.SetInteractable(false);
               editField.style.width = Length.Percent(85);
               editField.style.height = RootControl.EditFieldHeight;
               editField.style.marginTop = marginTop;
               subtypeFields.Add(editField);
               columnFour.Add(editField); 
           }  
           // subtypeCustomFields
           for (int i = 0; i < typingTableList[clickedTypeFieldIndex].Subtypes.Count; i++)
           {
               string customString = typingTableList[clickedTypeFieldIndex].Subtypes[i];
               if (typingTableList[clickedTypeFieldIndex].SubtypeCustoms != null)
                   if (typingTableList[clickedTypeFieldIndex].SubtypeCustoms.Count > i)
                       if (string.IsNullOrEmpty(typingTableList[clickedTypeFieldIndex].SubtypeCustoms[i]) == false)
                        customString = typingTableList[clickedTypeFieldIndex].SubtypeCustoms[i];
               EditField editField = new EditField(RootControl, false, "",customString, 
                   columnOne.resolvedStyle.width , null, ClickEnterCustomSubtype, null);
               editField.style.width = Length.Percent(85);
               editField.style.height = RootControl.EditFieldHeight;
               editField.style.marginTop = marginTop;
               subtypeCustomFields.Add(editField); 
               columnFive.Add(editField);
           }
           
           
           foreach (var typeField in typeFields)
               typeField.RegisterCallback<MouseUpEvent>(evt =>
               {
                   if (evt.button == 0)
                       ClickTypeField(typeFields.IndexOf(typeField));
               });

           ClickTypeField(clickedTypeFieldIndex, true);
           
           columnsRow.Add(columnOne);
           columnsRow.Add(columnTwo);
           columnsRow.Add(columnThree);
           columnsRow.Add(columnFour);
           columnsRow.Add(columnFive);
           customizeContent.Add(columnsRow);
       }
       private void ClickEnterCustomType(EditField arg1, string arg2)
       {
           RootControl.RootEdit.MakeTableTypingChange(typingTableList[typeCustomFields.IndexOf(arg1)], "TypeCustom", arg2);
      //   typingTableList[typeCustomFields.IndexOf(arg1)].TypeCustom = arg2;
           if (arg2 == "") 
               RebuildCustomization();
           arg1.SetColor(new Color(RootControl.BlueElementColor.r,RootControl.BlueElementColor.g,RootControl.BlueElementColor.b, RootControl.BlueElementColor.a * 2f));
       }
       private void ClickEnterCustomSubtype(EditField arg1, string arg2)
       {
           int index = subtypeCustomFields.IndexOf(arg1);
           TableTyping tableTyping = typingTableList[clickedTypeFieldIndex];
           Debug.Log("Table: " + tableTyping.Supertype);
           RootControl.RootEdit.MakeTableTypingChange(tableTyping, "SubtypeCustoms", arg2, index);
           typingTableList[clickedTypeFieldIndex].Subtypes[index] = arg2;

           RootControl.RegisterViewWindowValueChange(false);
           if (arg2 == "")
               RebuildCustomization();
           arg1.SetColor(new Color(RootControl.BlueElementColor.r,RootControl.BlueElementColor.g,RootControl.BlueElementColor.b, RootControl.BlueElementColor.a * 2f));
       }


      
       private int clickedTypeFieldIndex = 0;
       private void ClickTypeField(  int index, bool skipRebuild = false)
       {
           clickedTypeFieldIndex = index;
           foreach (var typeField in typeFields)
             typeField.SetInteractable(false);
           typeFields[index].SetColor(new Color(Color.green.r,Color.green.g,Color.green.b,  0.3f));
           if (skipRebuild == false)
              RebuildCustomization();
       }
        

       private VisualElement CreateEditfieldColumn(float width)
       {
           VisualElement newElement = new VisualElement();
           newElement.style.width = Length.Percent(width);
           newElement.style.height = Length.Percent(100);
           return newElement;
       } 
       private void ClearEditFields()
       {
           if (typeFields != null)
               foreach (var typeField in typeFields)
                   if (typeField != null)
                       typeField.RemoveFromHierarchy();
           typeFields = new List<EditField>();
           if (typeCustomFields != null)
               foreach (var typeField in typeCustomFields)
                   if (typeField != null)
                       typeField.RemoveFromHierarchy();
           typeCustomFields = new List<EditField>();
           if (subtypeFields != null)
               foreach (var typeField in subtypeFields)
                   if (typeField != null)
                       typeField.RemoveFromHierarchy();
           subtypeFields = new List<EditField>();
           if (subtypeCustomFields != null)
               foreach (var typeField in subtypeCustomFields)
                   if (typeField != null)
                       typeField.RemoveFromHierarchy();
           subtypeCustomFields = new List<EditField>();
       }
       private void CreateHeaderLabels()
       {
           VisualElement labelsRow = new VisualElement();
           labelsRow.style.width = Length.Percent(100);
           labelsRow.style.height = 20f;
           labelsRow.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
           labelsRow.style.marginTop = 4f;
           VisualElement blockerOne = new VisualElement();
           VisualElement blockerTwo = new VisualElement();
           VisualElement blockerThree = new VisualElement();
           blockerOne.style.width = Length.Percent(18);
           blockerTwo.style.width = Length.Percent(43);
           blockerThree.style.width = Length.Percent(23);
           blockerOne.style.flexShrink = 1;
           blockerTwo.style.flexShrink = 1;
           blockerThree.style.flexShrink = 1;

           Label labelOne = new Label("Supertypes");
           labelOne.style.fontSize = 14f;
           labelOne.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold); 
           string subTypeString =  typingTableList[clickedTypeFieldIndex].Supertype +  " Subtypes"  ;
           Label labelTwo = new Label(subTypeString);
           labelTwo.style.fontSize = 14f;
           labelTwo.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
           labelsRow.Add(blockerOne);
           labelsRow.Add(labelOne);
           labelsRow.Add(blockerTwo);
           labelsRow.Add(labelTwo);
           labelsRow.Add(blockerThree);
           customizeContent.Add(labelsRow);
       }
 
       
       
       
       // account for subsubtyping  
       private void CreateCustomizeContentForMatterTyping()
       {
           
       }

       // account for traits and other character specifics
       private void CreateCustomizeContentForCharacterTyping()
       {
       
       }

      

       
       // Create dropdown EditField, manually assign for Type and Subtype
       // For Location, allow re-templating/naming of main Types
       // Save Templating in LocationTyping DB, load it in EditWindow



       private void CreateCustomizeContent()
       {
          
           BlueDropdown tester = new BlueDropdown(RootControl, 120, s => {});
           customizeContent.Add(tester);
       }
      
     

       private bool SingleTableSelected()
       {
 
           int count = 0;
           List<Selector> selectors = CategorySelectors.GetSelectors().ToList();
           selectors.AddRange(ElementSelectorsOne.GetSelectors());
           selectors.AddRange(ElementSelectorsTwo.GetSelectors());
           selectors.AddRange(ElementSelectorsThree.GetSelectors());
           selectors.AddRange(ElementSelectorsFour.GetSelectors());
           selectors.AddRange(ElementSelectorsFive.GetSelectors());

           
           foreach (var givenSelector in selectors)
           {
               if (givenSelector.Activated)
               {
                   singleSelectedSelector = givenSelector;
                   count++;
               }
           } 
           return  count == 1; 
       }
        
       private ListView resultsBrowser;
       private VisualElement resultsBrowserContainer;
       List<Element> ListOne = new List<Element>();     
       List<Element> ListTwo = new List<Element>();     
       List<Element> ListThree = new List<Element>();     
       private void RefreshResults()
       {
           if (RootControl.World == null)
               return;
           if (resultsBrowserContainer == null)
               resultsBrowserContainer = new VisualElement();
           resultsBrowserContainer.Clear();
           
           FillElementLists();
           ApplyDropdowns();
           IntegrateResultsBrowser();
       }
     
   private void ApplyDropdowns()
{
    string selectedField = fieldDropdown.value;
    string selectedOrder = orderDropdown.value;

    // Create a Func<Element, object> that gets the value of the selected field from an element
    Func<Element, object> selector = element => 
    {
        if (element == null) 
        {
            return null;
        }
        var prop = element.GetType().GetProperty(selectedField, BindingFlags.Public | BindingFlags.Instance);
        return prop?.GetValue(element);
    };

    // Check if the selected field is of type string
    var isString = selector(ListOne.FirstOrDefault()) is string;

    // Sort the lists
    if (selectedOrder == "Ascend")
    {
        if (isString)
        {
            ListOne = ListOne.OrderBy(selector).ToList();
            ListTwo = ListTwo.OrderBy(selector).ToList();
            ListThree = ListThree.OrderBy(selector).ToList();
        }
        else
        {
            ListOne = ListOne.OrderBy(element => Convert.ToDouble(selector(element))).ToList();
            ListTwo = ListTwo.OrderBy(element => Convert.ToDouble(selector(element))).ToList();
            ListThree = ListThree.OrderBy(element => Convert.ToDouble(selector(element))).ToList();
        }
    }
    else if (selectedOrder == "Descend")
    {
        if (isString)
        {
            ListOne = ListOne.OrderByDescending(selector).ToList();
            ListTwo = ListTwo.OrderByDescending(selector).ToList();
            ListThree = ListThree.OrderByDescending(selector).ToList();
        }
        else
        {
            ListOne = ListOne.OrderByDescending(element => Convert.ToDouble(selector(element))).ToList();
            ListTwo = ListTwo.OrderByDescending(element => Convert.ToDouble(selector(element))).ToList();
            ListThree = ListThree.OrderByDescending(element => Convert.ToDouble(selector(element))).ToList();
        }
    }

    // Filter the lists
    if (selectedOrder == "Include")
    {
        ListOne = ListOne.Where(element => selector(element) != null && !selector(element).Equals(0)).ToList();
        ListTwo = ListTwo.Where(element => selector(element) != null && !selector(element).Equals(0)).ToList();
        ListThree = ListThree.Where(element => selector(element) != null && !selector(element).Equals(0)).ToList();
    }
    else if (selectedOrder == "Exclude")
    {
        ListOne = ListOne.Where(element => selector(element) == null || selector(element).Equals(0)).ToList();
        ListTwo = ListTwo.Where(element => selector(element) == null || selector(element).Equals(0)).ToList();
        ListThree = ListThree.Where(element => selector(element) == null || selector(element).Equals(0)).ToList();
    }
}



       private void IntegrateResultsBrowser()
       { 
           resultsBrowserContainer.Add(CreateResultsBrowser());
           resultsContainer.Add(resultsBrowserContainer); 
       }
       private ListView CreateResultsBrowser()
       {
           List<Element> allResults = new List<Element>();
           allResults.AddRange(ListOne);
           allResults.AddRange(ListTwo);
           allResults.AddRange(ListThree); 

           VisualElement MakeItem() => new ResultRowElement();

           void BindItem(VisualElement e, int i)
           {
               ResultRowElement row = (ResultRowElement)e;
               Element element = allResults[i];
               row.UploadElement(element, RootControl, i);

               // Color the row text based on the original list of the element
               if (ListOne.Contains(element))
                   row.Label.style.color = RootControl.ResultsColorResultTextOne;
               else if (ListTwo.Contains(element))
                   row.Label.style.color = RootControl.ResultsColorResultTextTwo;
               else if (ListThree.Contains(element))
                   row.Label.style.color = RootControl.ResultsColorResultTextThree;

               if (RootControl.Element != null && element == RootControl.Element)
                   row.Highlight(1);

               e.RegisterCallback<ClickEvent>(evt => ClickResultRow(evt, element));
               e.RegisterCallback<ContextClickEvent>(evt => RightClickResultRow( element)); 
               e.RegisterCallback<MouseEnterEvent>(evt => HoverResultRow( element));
               e.RegisterCallback<MouseLeaveEvent>(evt => ExitHoverResultRow( element));
           }
           ListView listView =new ListView(allResults, RootView.ResultRowHeight, MakeItem, BindItem)
           {
               selectionType = SelectionType.Single,
               reorderable = true,
               style =
               { 
                   height = allResults.Count *  RootView.ResultRowHeight   ,
               }
                
           }; 

           return listView;
       }

       private void HoverResultRow(Element element)
       {
            //todo highlight pinelement if there is one for element
       }
       private void ExitHoverResultRow(Element element)
       {
       }

       private const float DoubleClickInterval = 0.3f;
       private float lastClickTime;
       private Element lastClickedElement;
       private void ClickResultRow(ClickEvent evt, Element containedElement)
       {
           rootVisualElement.Focus();
           float currentTime = (float)EditorApplication.timeSinceStartup; 
         
           if (currentTime - lastClickTime < DoubleClickInterval && lastClickedElement == containedElement) // double click
           {
               RootControl.SetElement(containedElement);
               RootControl.RootMap.AttemptSelectAndCenterCameraOnPinForElement(RootControl.Element);
           }
           else // single click
           {
               RootControl.SetElement(containedElement);
           }
    
           lastClickTime = currentTime;
           lastClickedElement = containedElement;
       } 
       private void RightClickResultRow(Element element)
       {
           RootControl.SetElement(null);
       }

          private void FillElementLists()
       {
           ListOne = new List<Element>();     
           ListTwo = new List<Element>();     
           ListThree = new List<Element>();
           

           // add activated element names to list
           List<string> includedTableNames = (from categorySelector in CategorySelectors.GetCategorySelectors() where categorySelector.Activated select categorySelector.name).ToList();
           includedTableNames.AddRange(from elementSelector in ElementSelectorsOne.GetElementSelectors() where elementSelector.Activated select elementSelector.name);
           includedTableNames.AddRange(from elementSelector in ElementSelectorsTwo.GetElementSelectors() where elementSelector.Activated select elementSelector.name);
           includedTableNames.AddRange(from elementSelector in ElementSelectorsThree.GetElementSelectors() where elementSelector.Activated select elementSelector.name);
           includedTableNames.AddRange(from elementSelector in ElementSelectorsFour.GetElementSelectors() where elementSelector.Activated select elementSelector.name);
           includedTableNames.AddRange(from elementSelector in ElementSelectorsFive.GetElementSelectors() where elementSelector.Activated select elementSelector.name);

           // parse to element table types
           List<Element.Table> includedTables = includedTableNames.Select(includedTableName => (Element.Table)Enum.Parse(typeof(Element.Table), includedTableName)).ToList();
           List<Element> potentialElements = includedTables.SelectMany(table => RootControl.GetElementsOfTable(table)).ToList();
        
 
           if (string.IsNullOrEmpty(searchText) || searchText == "...")  // no search text, put everything in list one
               ListOne = potentialElements.ToList();
           else
               RunThroughSearch(potentialElements);
       }
          
          private string searchText = "";
          private void OnSearchEnterPress(string value)
          {
              if (RootView.searchOnPressEnter == false)
                  return;

              searchText = value;
              ProcessQuery(searchText); 
              RefreshResults();

          }
          private void OnTextChanged(string value)
          {  
              if (RootView.searchOnPressEnter == true)
                  return;
              Debug.Log("Hi");

              searchText = value;
              ProcessQuery(searchText); 
              RefreshResults();
          }
          
          List<Element> availableElements = new List<Element>();
          List<string> identifiedNames = new List<string>(); // word
          List<string> identifiedCombos = new List<string>(); // word:word    eg age:<60   location:london trait:mean trait:scary
          //example query: 'Mrspooky Misscranky age:<60 location:london trait:mean trait:scary' 
          private void ProcessQuery(string query)
          {
              identifiedNames.Clear();
              identifiedCombos.Clear();
              var tokens = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
              foreach(var token in tokens)
              {
                  if (token.Contains(":") && !token.StartsWith(":") && !token.EndsWith(":"))
                      identifiedCombos.Add(token);
                  else
                      identifiedNames.Add(token);
              } 
          }
          
          // produce a ListOne,ListTwo,ListThree distribution
          private void RunThroughSearch(List<Element> potentialElements)
          {
              ListOne = new List<Element>();
              ListTwo = new List<Element>();
              ListThree = new List<Element>(); 
              availableElements = potentialElements.ToList(); 
    
              if (identifiedNames.Count > 0)
                  FilterByNames(identifiedNames);
              if (identifiedCombos.Count > 0)
                  FilterByProperties(identifiedCombos); 
 
              ListTwo.AddRange(availableElements);
          }

          
          private void FilterByNames(List<string> list)
          {
              foreach (var name in list)
              {
                  for (int i = 0; i < availableElements.Count; i++)
                  {
                      int indexOfSubstring = availableElements[i].Name.IndexOf(name, StringComparison.OrdinalIgnoreCase);

                      // Check if the substring is found and is at least 3 characters long
                      if (indexOfSubstring >= 0 && name.Length >= 3)
                      {
                          ListOne.Add(availableElements[i]);
                          availableElements.RemoveAt(i);
                          i--;  
                      }
                  }
              }
          }

       private void FilterByProperties(List<string> combos)
       {
           foreach(var combo in combos)
           {
               Debug.Log("checking : " + combo);
               var parts = combo.Split(':');
               var field =FirstCharToUpper( parts[0].ToLower());
               var condition = parts[1];

               for(int i = 0; i < availableElements.Count;)
               {
                   bool hasProperty = HasProperty(availableElements[i], field);
                   if (hasProperty)
                   { 
                       bool matchesCondition = EvaluateCondition(availableElements[i], field, condition);
                       if (matchesCondition)
                       { 
                           ListOne.Add(availableElements[i]);
                           availableElements.RemoveAt(i);  
                       }
                       else
                       { 
                           ListThree.Add(availableElements[i]);
                           availableElements.RemoveAt(i);  
                       }
                   }
                   else
                   { 
                       {
                           ListTwo.Add(availableElements[i]);
                           availableElements.RemoveAt(i);  
                       }
                   }
                 
               }
           }
       }
       private string FirstCharToUpper(string input)
       {
           if (string.IsNullOrEmpty(input))
               throw new ArgumentException("ARGH!");

           return input.First().ToString().ToUpper() + input.Substring(1).ToLower();
       }
       private bool HasProperty(Element element, string field)
       {
           var type = element.GetType();
           var property = type.GetProperty(field, BindingFlags.Public | BindingFlags.Instance);

           if (property == null)
           {
               Debug.Log("hasproperty return null for " + element.Name + "  " + field );
               return false; // if there's no such property
           }

           return true;
       }
       private bool EvaluateCondition(Element element, string field, string condition)
       {
           var type = element.GetType();
           var property = type.GetProperty(field);
    
           if (property == null) return false;  // No such property exists

           var value = property.GetValue(element);
           if (value == null) return false; // If the property's value is null

           string stringValue = value.ToString();

           // Evaluate for not equal condition
           if (condition.StartsWith("!"))
           {
               return !stringValue.Equals(condition.Substring(1), StringComparison.OrdinalIgnoreCase);
           }
    
           // Evaluate for numerical conditions
           if (double.TryParse(stringValue, out double numValue))
           {
               Debug.Log("trypare: " + condition);
               if (condition.StartsWith("<"))
               {
                   if (double.TryParse(condition.Substring(1), out double conditionValue))
                   {
                       Debug.Log(" StartsWith <  trypare:     return numValue < conditionValue"  );
                       return numValue < conditionValue;
                   }
               }
               else if (condition.StartsWith(">"))
               {
                   if (double.TryParse(condition.Substring(1), out double conditionValue))
                   {
                       Debug.Log(" StartsWith > trypare:     return numValue < conditionValue"  );
                       return numValue > conditionValue;
                   }
               }
               else
               {
                   if (double.TryParse(condition, out double conditionValue))
                   {
                       Debug.Log(" .TryParse(condition, out double conditionValu"  );

                       return numValue == conditionValue;
                   }
               }
           }

           // Default string equality check
           return stringValue.Equals(condition, StringComparison.OrdinalIgnoreCase);
       }
          
    
       
       
       
       
       // search: ':' indicates property? only for single element selection, or even for all? without : defaults to name
       // click to see list of suggested fields for current selectors? 

       // modifiers: 
       // Search bar has terms
       //   - name results WHITE
       //   - in Events  OFFWHITE
       //   - in Relations OFFWHITE
       //   - name results for deactivates DARK    
       // No Search Bar Use
       //   - sorted results WHITE
       //   -
       // list types: PRIMARY,SECONDARY,TERTIARY
       
       
       
       private void BuildCustomizeRoot()
       {
           customizeContainer.Add(customizeContent);
           customizeContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
       }

       
       
       
   
     
    #endregion
    
    
    private MonoLoader _monoLoader;
    private MonoLoader MonoLoader
    {
        get
        {
            if (_monoLoader == null)
                _monoLoader = FindObjectOfType<MonoLoader>(true);
            if (_monoLoader == null)
                Debug.LogWarning("! No MonoLoader GameObject found. Please re-load the tool from Launcher.");
            return _monoLoader;
        }
    }     
    private RootControl _rootControl;
    private RootControl RootControl
    {
        get
        {
            if (_rootControl == null)
                _rootControl = LoadRootControl();
            return _rootControl;
        }
    }
    private RootControl LoadRootControl()
    {
        RootControl rootControl =  AssetDatabase.LoadAssetAtPath<RootControl>(MonoLoader.projectPath + MonoLoader.rootPath + "RootControl.asset");
        if (rootControl == null)
            Debug.LogWarning("! No RootControl found. Please re-load the tool from Launcher.");
        return rootControl;
    }
    private RootView _rootView;
    private RootView RootView
    {
        get
        {
            if (_rootView == null)
                _rootView = LoadRootView();
            return _rootView;
        }
    }
    private RootView LoadRootView()
    {
        RootView rootView =  AssetDatabase.LoadAssetAtPath<RootView>(MonoLoader.projectPath + MonoLoader.rootPath + "RootView.asset");
        if (rootView == null)
            Debug.LogWarning("! No RootView found. Please re-load the tool from Launcher.");
        return rootView;
    }
}
