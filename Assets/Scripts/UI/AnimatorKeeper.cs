using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Found from https://forum.unity.com/threads/bool-parameter-does-not-persist-through-deactivation-reactivation-except-if-set-in-editor.287955/

public class AnimatorKeeper : MonoBehaviour
{
    private class AnimatorParameterValue
    {
        public AnimatorControllerParameterType m_Type;
        public float m_Float;
        public bool m_Bool;
        public int m_Int;
    }

    private class AnimatorLayerState
    {
        public int m_ShortNameHash = 0;
        public float m_NormalizedTime;
    }

    private Animator m_Animator;

    private Dictionary<int, AnimatorParameterValue> m_Parameters;
    private AnimatorLayerState[] m_LayerStates;

    private void Init()
    {
        m_Animator = GetComponent<Animator>();

        m_Parameters = new Dictionary<int, AnimatorParameterValue>();
        foreach (AnimatorControllerParameter acp in m_Animator.parameters)
        {
            if (acp.type != AnimatorControllerParameterType.Trigger)
            {
                AnimatorParameterValue apv = new AnimatorParameterValue();
                apv.m_Type = acp.type;
                m_Parameters.Add(acp.nameHash, apv);
            }
        }
        m_LayerStates = new AnimatorLayerState[m_Animator.layerCount];
        for (int lidx = 0; lidx < m_LayerStates.Length; ++lidx)
        {
            m_LayerStates[lidx] = new AnimatorLayerState();
        }
    }

    private void OnEnable()
    {
        if (m_Animator == null)
        {
            Init();
        }
        if (m_Animator != null)
        {
            // HANDLES THE CASE OF DEV CONSOLE ENDING THE ROUND
            if (GameManager.instance && GameManager.instance.wasConsoleUnpause)
            {
                m_Animator.Rebind();
                m_Animator.Update(0f);

                GameManager.instance.wasConsoleUnpause = false;
                return;
            }

            foreach (KeyValuePair<int, AnimatorParameterValue> kvp in m_Parameters)
            {
                switch (kvp.Value.m_Type)
                {
                    case AnimatorControllerParameterType.Bool:
                        m_Animator.SetBool(kvp.Key, kvp.Value.m_Bool);
                        break;
                    case AnimatorControllerParameterType.Float:
                        m_Animator.SetFloat(kvp.Key, kvp.Value.m_Float);
                        break;
                    case AnimatorControllerParameterType.Int:
                        m_Animator.SetInteger(kvp.Key, kvp.Value.m_Int);
                        break;
                }
            }

            for (int lidx = 0; lidx < m_LayerStates.Length; ++lidx)
            {
                m_Animator.Play(m_LayerStates[lidx].m_ShortNameHash, lidx, m_LayerStates[lidx].m_NormalizedTime);
            }
        }
    }

    private void OnDisable()
    {
        if (m_Animator == null)
        {
            Init();
        }
        if (m_Animator != null)
        {
            foreach (KeyValuePair<int, AnimatorParameterValue> kvp in m_Parameters)
            {
                switch (kvp.Value.m_Type)
                {
                    case AnimatorControllerParameterType.Bool:
                        kvp.Value.m_Bool = m_Animator.GetBool(kvp.Key);
                        break;
                    case AnimatorControllerParameterType.Float:
                        kvp.Value.m_Float = m_Animator.GetFloat(kvp.Key);
                        break;
                    case AnimatorControllerParameterType.Int:
                        kvp.Value.m_Int = m_Animator.GetInteger(kvp.Key);
                        break;
                }
            }

            for (int lidx = 0; lidx < m_LayerStates.Length; ++lidx)
            {
                AnimatorStateInfo asi = m_Animator.GetCurrentAnimatorStateInfo(lidx);
                m_LayerStates[lidx].m_ShortNameHash = asi.shortNameHash;
                m_LayerStates[lidx].m_NormalizedTime = asi.normalizedTime;
            }
        }
    }
}
