using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


// represents the line, and possibly other elements that stay the same on zoom/pan

public class Timeline : VisualElement
{

    private RootControl RootControl;
    private VisualElement parentElement;
    private VisualElement lineContainer; 
    private VisualElement horizontalLine;

    public Timeline(RootControl RootControl, VisualElement parentElement)
    {
        this.RootControl = RootControl;
        this.parentElement = parentElement;

        StyleBase();
        BuildComponents();


    }
    private void StyleBase()
    {
        /*style.width = Length.Percent(100);
        style.height = Length.Percent(100);*/

        /*// Make the Timeline act as a flex container with column direction
        style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
        style.alignItems = new StyleEnum<Align>(Align.Center); // For horizontal alignment in column direction
        style.justifyContent = new StyleEnum<Justify>(Justify.Center); // For vertical alignment in column direction*/
    }

private void BuildComponents()
{
    lineContainer = new VisualElement();
    lineContainer.style.position = Position.Absolute;
    lineContainer.style.width = Length.Percent(100);
    lineContainer.style.height = Length.Percent(100); 
  //  lineContainer.style.marginTop = new StyleLength(StyleKeyword.Auto);
  //  lineContainer.style.marginBottom = new StyleLength(StyleKeyword.Auto);
     
  lineContainer.style.marginTop = parentElement.resolvedStyle.height / 2f;  

    CreateHorizontalLine(); 

    this.Add(lineContainer);
}

 
 

private void CreateHorizontalLine()
{
    // Create the main horizontal line
    horizontalLine = new VisualElement();
    horizontalLine.style.height = new StyleLength(2f); // 2 pixels high
    horizontalLine.style.backgroundColor = Color.black; // or your desired color
    horizontalLine.style.width = Length.Percent(100);
    horizontalLine.style.position = Position.Absolute; // Set this to Absolute so that it stays at the top of the lineContainer
    horizontalLine.style.top = new StyleLength(RootControl.RootTimeline.timeBarHeight/2f); // Center it vertically in the lineContainer
    lineContainer.Add(horizontalLine);

}





}
