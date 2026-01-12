using UnityEngine;

public class FoldoutAttribute : PropertyAttribute
{
    public string name;

    public FoldoutAttribute(string name)    {
        this.name = name;
    }
}