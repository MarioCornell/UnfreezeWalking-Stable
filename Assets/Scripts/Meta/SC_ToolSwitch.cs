using UnityEngine;
using Battlehub.RTCommon;


public class SC_ToolSwitch : MonoBehaviour
{
    public void SwitchToScaleTool()
    {
        IRTE m_editor = IOC.Resolve<IRTE>();
        m_editor.Tools.Current = RuntimeTool.Scale;
    }
}