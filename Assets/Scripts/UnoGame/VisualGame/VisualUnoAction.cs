using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class VisualUnoAction: MonoBehaviour
{
    private UnoColor _colorSelected = UnoColor.Wild;
    public GameObject selectColorGo;

    public async Task<UnoColor> SelectColor()
    {
        selectColorGo.SetActive(true);
        _colorSelected = UnoColor.Wild;
        while (_colorSelected == UnoColor.Wild)
        {
            await Task.Yield();
        }
        selectColorGo.SetActive(false);
        return _colorSelected;
    }

    public void SetColor(int numColor)
    {
        /*  0 == Red,
            1 == Blue,
            2 == Yellow,
            3 == Green,
            4 == Wild   */
        _colorSelected = (UnoColor)numColor;
    }
}
