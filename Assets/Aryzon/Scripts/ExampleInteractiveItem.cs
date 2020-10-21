using UnityEngine;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Examples
{
    // This script is a simple example of how an interactive item can
    // be used to change things on gameobjects by handling events.
	public class ExampleInteractiveItem : MonoBehaviour
    {
        [SerializeField] private VRInteractiveItem m_InteractiveItem;
		[SerializeField] private Renderer m_Renderer;
		[SerializeField] private Material m_OverMaterial;
		[SerializeField] private Material m_NormalMaterial;
		[SerializeField] private Material m_ClickedMaterial;


		private void Update () {

		}

        private void Awake ()
        {
			m_NormalMaterial = m_Renderer.material;
        }


        private void OnEnable()
        {
			Debug.Log("Enable");
            m_InteractiveItem.OnOver += HandleOver;
            m_InteractiveItem.OnOut += HandleOut;
            m_InteractiveItem.OnClick += HandleClick;
        }


        private void OnDisable()
        {
			Debug.Log("Disable");
            m_InteractiveItem.OnOver -= HandleOver;
            m_InteractiveItem.OnOut -= HandleOut;
            m_InteractiveItem.OnClick -= HandleClick;
        }


        //Handle the Over event
        private void HandleOver()
        {
            Debug.Log("Show over state");
            m_Renderer.material = m_OverMaterial;
        }


        //Handle the Out event
        private void HandleOut()
        {
            Debug.Log("Show out state");
            m_Renderer.material = m_NormalMaterial;
        }


        //Handle the Click event
        private void HandleClick()
        {
            Debug.Log("Show click state");
            m_Renderer.material = m_ClickedMaterial;
        }
			
    }
}