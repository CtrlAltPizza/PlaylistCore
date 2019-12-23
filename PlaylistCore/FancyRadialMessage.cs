using SiaUtil.Visualizers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlaylistCore
{
    public class FancyRadialMessage : MonoBehaviour
    {
        public void SetupAndDisplayThenHide(string text, Vector3 pos, Quaternion rot)
        {
            WorldSpaceRadial radial = WorldSpaceRadial.Create();
            radial.transform.position = pos;
            radial.transform.rotation = rot;
            radial.Progress = 0;
            radial.Text = "\n\n\n\n\n" + text;

            SharedCoroutineStarter.instance.StartCoroutine(LoadRadial(radial));
        }

        private IEnumerator FadeIn(WorldSpaceRadial radial)
        {
            float timer = 0;
            var rimage = radial.GetComponent<Image>();
            var timage = radial.GetComponent<TextMeshProUGUI>();
            while (timer < 1)
            {
                yield return new WaitForSeconds(.01f);
                timer += .05f;
                rimage.color = new Color(1f, 1f, 1f, timer);
                timage.color = new Color(1f, 1f, 1f, timer);
            }
        }

        private IEnumerator FadeOut(WorldSpaceRadial radial)
        {
            float timer = 1;
            var rimage = radial.GetComponent<Image>();
            var timage = radial.GetComponent<TextMeshProUGUI>();
            while (timer > 0)
            {
                yield return new WaitForSeconds(.01f);
                timer -= .05f;
                rimage.color = new Color(1f, 1f, 1f, timer);
                timage.color = new Color(1f, 1f, 1f, timer);
            }
        }

        private IEnumerator LoadRadial(WorldSpaceRadial radial)
        {
            float timer = 0f;
            //SharedCoroutineStarter.instance.StartCoroutine(FadeIn(radial));
            while (timer < .8f)
            {
                yield return new WaitForSeconds(.01f);
                timer += .01f;
                radial.Progress = timer;
            }
            while (timer < 1f)
            {
                yield return new WaitForSeconds(.01f);
                timer += .005f;
                radial.Progress = timer;
            }
            yield return new WaitForSeconds(2f);
            while (timer > .2f)
            {
                yield return new WaitForSeconds(.01f);
                timer -= .01f;
                radial.Progress = timer;
            }
            //SharedCoroutineStarter.instance.StartCoroutine(FadeOut(radial));
            while (timer > 0f)
            {
                yield return new WaitForSeconds(.01f);
                timer -= .005f;
                radial.Progress = timer;
            }
            radial.Text = "";
            yield return new WaitForSeconds(1f);
            Destroy(radial);
            Destroy(this);
        }
    }
}
