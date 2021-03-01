﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Text.RegularExpressions;

namespace InGameDebugger {
	public class SceneViewer : MonoBehaviour {
		public Font font;
		public List<EditGameObject> editGameObjects = new List<EditGameObject>();
		private GameObject[] editLines;
		private GameObject inspectorObj;

		private GameObject topPanel;
		private GameObject refreshBtn;

		private GameObject scrollViewH;
		private GameObject viewportH;
		private GameObject contentH;

		private GameObject scrollViewI;
		private GameObject viewportI;
		private GameObject contentI;
		private GameObject backBtn;

		private MeshViewer meshViewer;
		private AudioSource mAudioSource;

		public float size = 40f;
		public int fontSize = 25;
		private bool find;
		private float offset;
		private GameObject comBtn;

		private void Start() {
			try {
				font = Font.CreateDynamicFontFromOSFont("Arial", 50);

				topPanel = new GameObject("TopPanel");
				topPanel.AddComponent<SceneViewerFlag>();
				topPanel.transform.SetParent(transform);
				topPanel.AddComponent<Image>();
				topPanel.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
				topPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
				topPanel.GetComponent<RectTransform>().anchorMax = Vector2.one;
				topPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
				topPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(300, 100);
				topPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width - 300);

				refreshBtn = new GameObject("RefreshBtn");
				refreshBtn.AddComponent<SceneViewerFlag>();
				refreshBtn.transform.SetParent(topPanel.transform);
				refreshBtn.AddComponent<Image>();
				refreshBtn.AddComponent<Button>();
				refreshBtn.GetComponent<RectTransform>().anchorMin = Vector2.one;
				refreshBtn.GetComponent<RectTransform>().anchorMax = Vector2.one;
				refreshBtn.GetComponent<RectTransform>().pivot = Vector2.one;
				refreshBtn.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				refreshBtn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 150);

				var refreshText = new GameObject("RefreshText");
				refreshText.AddComponent<SceneViewerFlag>();
				refreshText.transform.SetParent(refreshBtn.transform);
				refreshText.AddComponent<Text>();
				refreshText.GetComponent<Text>().text = "刷新";
				refreshText.GetComponent<Text>().font = font;
				refreshText.GetComponent<Text>().fontSize = 50;
				refreshText.GetComponent<Text>().color = Color.black;
				refreshText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
				refreshText.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				refreshText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 150);

				var findBtn = Instantiate(refreshBtn, topPanel.transform);
				findBtn.name = "FindBtn";
				findBtn.GetComponentInChildren<Text>().text = "捕捉";
				findBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-160, 0);
				findBtn.GetComponent<Button>().onClick.AddListener(new UnityAction(() => {
					scrollViewH.SetActive(false);
					find = true;
				}));
				refreshBtn.GetComponent<Button>().onClick.AddListener(new UnityAction(RefreshGameObjects));

				scrollViewH = new GameObject("ScrollViewH");
				scrollViewH.AddComponent<SceneViewerFlag>();
				scrollViewH.transform.SetParent(transform);
				scrollViewH.AddComponent<Image>();
				scrollViewH.GetComponent<Image>().color = new Color(1, 1, 1, 0);
				scrollViewH.GetComponent<RectTransform>().anchorMin = Vector2.zero;
				scrollViewH.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
				scrollViewH.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
				scrollViewH.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				scrollViewH.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
				scrollViewH.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height - 100);
				scrollViewH.AddComponent<ScrollRect>().onValueChanged.AddListener((_) => {
					RefreshView();
				});
				scrollViewH.GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;
				scrollViewH.GetComponent<ScrollRect>().decelerationRate = 0.5f;

				viewportH = new GameObject("ViewportH");
				viewportH.AddComponent<SceneViewerFlag>();
				viewportH.transform.SetParent(scrollViewH.transform);
				viewportH.AddComponent<Image>();
				viewportH.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
				viewportH.GetComponent<RectTransform>().anchorMin = Vector2.zero;
				viewportH.GetComponent<RectTransform>().anchorMax = Vector2.one;
				viewportH.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
				viewportH.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				viewportH.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
				viewportH.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height - 100);
				viewportH.AddComponent<Mask>();

				contentH = new GameObject("ContentH");
				contentH.AddComponent<SceneViewerFlag>();
				contentH.transform.SetParent(viewportH.transform);
				contentH.AddComponent<RectTransform>();
				contentH.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
				contentH.GetComponent<RectTransform>().anchorMax = Vector2.one;
				contentH.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
				contentH.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
				scrollViewH.GetComponent<ScrollRect>().viewport = viewportH.GetComponent<RectTransform>();
				scrollViewH.GetComponent<ScrollRect>().content = contentH.GetComponent<RectTransform>();

				scrollViewI = new GameObject("ScrollViewI");
				scrollViewI.AddComponent<SceneViewerFlag>();
				scrollViewI.transform.SetParent(transform);
				scrollViewI.AddComponent<Image>();
				scrollViewI.GetComponent<Image>().color = new Color(1, 1, 1, 0);
				scrollViewI.GetComponent<RectTransform>().anchorMin = Vector2.zero;
				scrollViewI.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
				scrollViewI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
				scrollViewI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				scrollViewI.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
				scrollViewI.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height - 100);
				scrollViewI.AddComponent<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;
				scrollViewI.GetComponent<ScrollRect>().decelerationRate = 0.5f;

				scrollViewI.SetActive(false);

				viewportI = new GameObject("ViewportI");
				viewportI.AddComponent<SceneViewerFlag>();
				viewportI.transform.SetParent(scrollViewI.transform);
				viewportI.AddComponent<Image>();
				viewportI.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
				viewportI.GetComponent<RectTransform>().anchorMin = Vector2.zero;
				viewportI.GetComponent<RectTransform>().anchorMax = Vector2.one;
				viewportI.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
				viewportI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				viewportI.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
				viewportI.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height - 100);
				viewportI.AddComponent<Mask>();

				contentI = new GameObject("ContentI");
				contentI.AddComponent<SceneViewerFlag>();
				contentI.transform.SetParent(viewportI.transform);
				contentI.AddComponent<RectTransform>();
				contentI.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
				contentI.GetComponent<RectTransform>().anchorMax = Vector2.one;
				contentI.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
				contentI.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
				scrollViewI.GetComponent<ScrollRect>().viewport = viewportI.GetComponent<RectTransform>();
				scrollViewI.GetComponent<ScrollRect>().content = contentI.GetComponent<RectTransform>();

				backBtn = new GameObject("BackBtn");
				backBtn.AddComponent<SceneViewerFlag>();
				backBtn.transform.SetParent(scrollViewI.transform);
				backBtn.AddComponent<Image>();
				backBtn.AddComponent<Button>();
				backBtn.GetComponent<RectTransform>().anchorMin = Vector2.one;
				backBtn.GetComponent<RectTransform>().anchorMax = Vector2.one;
				backBtn.GetComponent<RectTransform>().pivot = Vector2.one;
				backBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
				backBtn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 310);

				var backText = new GameObject("BackText");
				backText.AddComponent<SceneViewerFlag>();
				backText.transform.SetParent(backBtn.transform);
				backText.AddComponent<Text>();
				backText.GetComponent<Text>().text = "返回";
				backText.GetComponent<Text>().font = font;
				backText.GetComponent<Text>().fontSize = 50;
				backText.GetComponent<Text>().color = Color.black;
				backText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
				backText.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				backText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 310);
				backBtn.GetComponent<Button>().onClick.AddListener(() => {
					scrollViewI.SetActive(false);
					scrollViewH.SetActive(true);
				});

				var meshViewerObj = new GameObject("MeshViewer");
				meshViewerObj.transform.parent = transform;
				meshViewerObj.transform.position = new Vector3(1000, 1000, 990);
				meshViewerObj.AddComponent<Camera>();
				meshViewer = meshViewerObj.AddComponent<MeshViewer>();
				var light = new GameObject("Light");
				light.transform.parent = meshViewerObj.transform;
				light.AddComponent<Light>().type = LightType.Directional;
				var meshObj = new GameObject("Mesh");
				meshObj.transform.parent = meshViewerObj.transform;
				meshObj.transform.position = new Vector3(1000, 1000, 1000);
				meshObj.layer = 31;
				meshViewer.target = meshObj.transform;
				meshViewer.MeshFilter = meshObj.AddComponent<MeshFilter>();
				meshViewer.MeshRenderer = meshObj.AddComponent<MeshRenderer>();
				meshViewerObj.SetActive(false);

				mAudioSource = gameObject.AddComponent<AudioSource>();
				mAudioSource.playOnAwake = false;
				mAudioSource.loop = true;

				InitHierarchy();
				RefreshGameObjects();
				RefreshView();
			} catch (Exception e) {
				Utils.MessageBoxError(e.ToString(), "Error in SceneViewer Start");
			}
}

		private void RefreshView() {
			float y = 0;
			int num = 0, j = 0;
			float maxWidth = Screen.width;
			for (int i = 0; i < editGameObjects.Count; i++) {
				var edit = editGameObjects[i];
				if (IsLineShow(edit)) {
					if (y + size >= contentH.GetComponent<RectTransform>().anchoredPosition.y) {
						if (j < editLines.Length) {
							num++;
							editLines[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(edit.level * size, -y);
							var editline = editLines[j].GetComponent<EditLine>();
							editline.Set(edit);
							float width = editline.nameText.GetComponent<RectTransform>().rect.width + (edit.level + 1.5f) * size;
							if (width > maxWidth) {
								maxWidth = width;
							}
							if (!editLines[j].activeSelf) {
								editLines[j].SetActive(true);
							}
							j++;
						} else {
							break;
						}
					}
					y += size;
				}
			}
			for (; num < editLines.Length; num++) {
				editLines[num].SetActive(false);
			}
			foreach (var edit in editLines) {
				if (edit.activeSelf) {
					var editline = edit.GetComponent<EditLine>();
					editline.funcBtn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth - editline.editGameObject.level * size);
				}
			}
			contentH.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y);
			contentH.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
		}

		private bool IsLineShow(EditGameObject editline) {
			while (editline.father != null) {
				editline = editline.father;
				if (!editline.open) {
					return false;
				}
			}
			return true;
		}

		GameObject AddPropText(string propTextName) {
			var propText = new GameObject(comBtn.name);
			propText.transform.SetParent(comBtn.transform);
			propText.AddComponent<SceneViewerFlag>();
			var propTextT = propText.AddComponent<Text>();
			propTextT.text = $"{propTextName}:";
			propTextT.font = font;
			propTextT.fontSize = fontSize;
			propTextT.color = Color.black;
			propTextT.alignment = TextAnchor.MiddleLeft;
			var propTextR = propText.GetComponent<RectTransform>();
			propTextR.anchorMin = new Vector2(0, 1);
			propTextR.anchorMax = new Vector2(0, 1);
			propTextR.pivot = new Vector2(0, 1);
			propTextR.anchoredPosition = new Vector2(size * 0.6f, offset);
			propTextR.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width - size * 0.6f);
			propTextR.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size * 0.8f);
			return propText;
		}

		GameObject AddGroupText(string groupTextName) {
			var g = AddPropText(groupTextName);
			g.GetComponent<Text>().fontStyle = FontStyle.Bold;
			g.GetComponent<RectTransform>().anchoredPosition = new Vector2(size * 0.3f, offset);
			offset -= size;
			return g;
		}

		void AddVector3Editor(GameObject parent, Func<Vector3> get, Action<Vector3> set) {
			var vector3Editor = new GameObject($"{parent.name} Editor");
			vector3Editor.transform.SetParent(parent.transform);
			var v3R = vector3Editor.AddComponent<RectTransform>();
			v3R.anchorMin = Vector2.one;
			v3R.anchorMax = Vector2.one;
			v3R.pivot = Vector2.one;
			v3R.anchoredPosition = new Vector2(-20, 0);
			v3R.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
			v3R.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size * 3);
			var v3V = vector3Editor.AddComponent<Vector3Editor>();
			v3V.size = size;
			v3V.fontSize = fontSize;
			v3V.get = get;
			v3V.set = set;
			offset -= size * 3.2f;
		}

		void AddBoolEditor(GameObject parent, Func<bool> get, Action<bool> set) {
			var boolEditor = new GameObject($"{parent.name} Editor");
			boolEditor.transform.SetParent(parent.transform);
			var v3R = boolEditor.AddComponent<RectTransform>();
			v3R.anchorMin = Vector2.one;
			v3R.anchorMax = Vector2.one;
			v3R.pivot = Vector2.one;
			v3R.anchoredPosition = new Vector2(-20, 0);
			v3R.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size * 0.8f);
			v3R.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size * 0.8f);
			var v3B = boolEditor.AddComponent<BoolEditor>();
			v3B.size = size;
			v3B.fontSize = fontSize;
			v3B.get = get;
			v3B.set = set;
			offset -= size;
		}

		void AddEnumEditor(GameObject parent, Type enumType, Func<int> get, Action<int> set) {
			var enumEditor = new GameObject($"{parent.name} Editor");
			enumEditor.transform.SetParent(parent.transform);
			var enumR = enumEditor.AddComponent<RectTransform>();
			enumR.anchorMin = Vector2.one;
			enumR.anchorMax = Vector2.one;
			enumR.pivot = Vector2.one;
			enumR.anchoredPosition = new Vector2(-20, 0);
			enumR.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
			enumR.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
			var enumE = enumEditor.AddComponent<EnumEditor>();
			enumE.size = size;
			enumE.fontSize = fontSize;
			enumE.enumType = enumType;
			enumE.get = get;
			enumE.set = set;
			offset -= size * 1.2f;
		}

		void AddFloatEditor(GameObject parent, Func<float> get, Action<float> set, float min, float max) {
			var floatEditor = new GameObject($"{parent.name} Editor");
			floatEditor.transform.SetParent(parent.transform);
			var floatR = floatEditor.AddComponent<RectTransform>();
			floatR.anchorMin = Vector2.one;
			floatR.anchorMax = Vector2.one;
			floatR.pivot = Vector2.one;
			floatR.anchoredPosition = new Vector2(-20, 0);
			floatR.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
			floatR.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
			var floatV = floatEditor.AddComponent<FloatEditor>();
			floatV.size = size;
			floatV.fontSize = fontSize;
			floatV.get = get;
			floatV.set = set;
			floatV.minimum = min;
			floatV.maximum = max;
			offset -= size * 1.2f;
		}

		void AddFloatEditor(GameObject parent, Func<float> get, Action<float> set) {
			AddFloatEditor(parent, get, set, float.NegativeInfinity, float.PositiveInfinity);
		}

		void AddIntEditor(GameObject parent, Func<int> get, Action<int> set, float min, float max) {
			var intEditor = new GameObject($"{parent.name} Editor");
			intEditor.transform.SetParent(parent.transform);
			var intR = intEditor.AddComponent<RectTransform>();
			intR.anchorMin = Vector2.one;
			intR.anchorMax = Vector2.one;
			intR.pivot = Vector2.one;
			intR.anchoredPosition = new Vector2(-20, 0);
			intR.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
			intR.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
			var intV = intEditor.AddComponent<IntEditor>();
			intV.size = size;
			intV.fontSize = fontSize;
			intV.get = get;
			intV.set = set;
			intV.minimum = min;
			intV.maximum = max;
			offset -= size * 1.2f;
		}

		void AddIntEditor(GameObject parent, Func<int> get, Action<int> set) {
			AddIntEditor(parent, get, set, float.NegativeInfinity, float.PositiveInfinity);
		}

		void AddButton(GameObject parent, Func<string> get, Action onClick) {
			var button = new GameObject($"{parent.name} Button");
			button.transform.SetParent(parent.transform);
			var buttonR = button.AddComponent<RectTransform>();
			buttonR.anchorMin = Vector2.one;
			buttonR.anchorMax = Vector2.one;
			buttonR.pivot = Vector2.one;
			buttonR.anchoredPosition = new Vector2(-20, 0);
			buttonR.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
			buttonR.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
			button.AddComponent<Image>();
			var buttonT = new GameObject("text");
			buttonT.transform.SetParent(button.transform);
			var buttonTR = buttonT.AddComponent<RectTransform>();
			buttonTR.anchorMin = Vector2.zero;
			buttonTR.anchorMax = Vector2.one;
			buttonTR.anchoredPosition = Vector2.zero;
			buttonTR.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
			buttonTR.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
			var buttonTT = buttonT.AddComponent<Text>();
			buttonTT.color = Color.black;
			buttonTT.alignment = TextAnchor.MiddleCenter;
			buttonTT.font = font;
			buttonTT.fontSize = fontSize;
			buttonT.AddComponent<Updater>().Action = () => buttonTT.text = get();
			var buttonB = button.AddComponent<Button>();
			buttonB.onClick.AddListener(() => onClick?.Invoke());
			offset -= size * 1.2f;
		}

		void ViewMesh(MeshFilter meshFilter) {
			var activeH = viewportH.activeSelf;
			var activeI = viewportI.activeSelf;
			viewportH.SetActive(false);
			viewportI.SetActive(false);
			meshViewer.ViewMesh(meshFilter.sharedMesh, inspectorObj.GetComponent<MeshRenderer>().sharedMaterial);
			backBtn.SetActive(true);
			var button = backBtn.GetComponent<Button>();
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => {
				meshViewer.gameObject.SetActive(false);
				viewportH.SetActive(activeH);
				viewportI.SetActive(activeI);
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() => {
					scrollViewI.SetActive(false);
					scrollViewH.SetActive(true);
				});
			});
		}

		public void LoadInspector() {
			if (inspectorObj == null) {
				return;
			}
			foreach (Transform child in contentI.transform) {
				Destroy(child.gameObject);
			}
			float y = 0;
			var coms = inspectorObj.GetComponents<Component>();
			offset = -size;
			foreach (var com in coms) {
				offset = -size;
				comBtn = new GameObject(com.GetType().Name);
				comBtn.transform.SetParent(contentI.transform);
				comBtn.AddComponent<SceneViewerFlag>();
				comBtn.AddComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.4f);
				comBtn.AddComponent<Button>();
				var comBtnR = comBtn.GetComponent<RectTransform>();
				comBtnR.anchorMin = new Vector2(0, 1);
				comBtnR.anchorMax = new Vector2(0, 1);
				comBtnR.pivot = new Vector2(0, 1);
				comBtnR.anchoredPosition = new Vector2(0, y);
				comBtnR.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
				comBtnR.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
				var comText = new GameObject("Text");
				comText.transform.SetParent(comBtn.transform);
				var comTextT = comText.AddComponent<Text>();
				comTextT.text = com.GetType().Name;
				comTextT.font = font;
				comTextT.fontStyle = FontStyle.Bold;
				comTextT.fontSize = fontSize;
				comTextT.color = Color.black;
				comTextT.alignment = TextAnchor.MiddleLeft;
				var comTextR = comText.GetComponent<RectTransform>();
				comTextR.anchoredPosition = new Vector2(size * 0.3f, 0);
				comTextR.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
				comTextR.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
				y -= size;

				if (com is Transform transform) {
					AddVector3Editor(AddPropText("Position"), () => transform.position, v3 => transform.position = v3);
					AddVector3Editor(AddPropText("Rotation"), () => transform.rotation.eulerAngles, v3 => transform.rotation = Quaternion.Euler(v3));
					AddVector3Editor(AddPropText("Scale"), () => transform.localScale, v3 => transform.localScale = v3);
				} else if (com is MeshFilter meshFilter) {
					AddButton(AddPropText("Mesh"), () => meshFilter.sharedMesh == null ? "None" : meshFilter.sharedMesh.name, () => ViewMesh(meshFilter));
				} else if (com is MeshRenderer meshRenderer) {
					AddBoolEditor(AddPropText("enabled"), () => meshRenderer.enabled, b => meshRenderer.enabled = b);
					AddGroupText("Materials");
					var mats = meshRenderer.sharedMaterials;
					AddButton(AddPropText("Size"), () => mats.Length.ToString(), null);
					for (int i = 0; i < mats.Length; i++) {
						var mat = mats[i];
						AddButton(AddPropText($"Element {i}"), () => mat.name, null);
					}
					AddGroupText("Lighting");
					AddEnumEditor(AddPropText("Cast Shadows"), typeof(ShadowCastingMode), () => (int)meshRenderer.shadowCastingMode, i => meshRenderer.shadowCastingMode = (ShadowCastingMode)i);
					AddBoolEditor(AddPropText("Receive Shadows"), () => meshRenderer.receiveShadows, b => meshRenderer.receiveShadows = b);
					AddGroupText("Probes");
					AddEnumEditor(AddPropText("Light Probes"), typeof(LightProbeUsage), () => (int)meshRenderer.lightProbeUsage, i => meshRenderer.lightProbeUsage = (LightProbeUsage)i);
					AddEnumEditor(AddPropText("Reflection Probes"), typeof(ReflectionProbeUsage), () => (int)meshRenderer.reflectionProbeUsage, i => meshRenderer.reflectionProbeUsage = (ReflectionProbeUsage)i);
					AddButton(AddPropText("Anchor Override"), () => meshRenderer.probeAnchor == null ? "None" : meshRenderer.probeAnchor.name, null);
					AddGroupText("Addtional Settings");
					AddEnumEditor(AddPropText("Motion Vectors"), typeof(MotionVectorGenerationMode), () => (int)meshRenderer.motionVectorGenerationMode, i => meshRenderer.motionVectorGenerationMode = (MotionVectorGenerationMode)i);
					AddBoolEditor(AddPropText("Dynamic Occlusion"), () => meshRenderer.allowOcclusionWhenDynamic, b => meshRenderer.allowOcclusionWhenDynamic = b);
				} else if (com is BoxCollider boxCollider) {
					AddBoolEditor(AddPropText("enabled"), () => boxCollider.enabled, b => boxCollider.enabled = b);
					AddBoolEditor(AddPropText("Is Trigger"), () => boxCollider.isTrigger, b => boxCollider.isTrigger = b);
					AddButton(AddPropText("Material"), () => boxCollider.material == null ? "None" : boxCollider.material.name, null);
					AddVector3Editor(AddPropText("Center"), () => boxCollider.center, v3 => boxCollider.center = v3);
					AddVector3Editor(AddPropText("Size"), () => boxCollider.size, v3 => boxCollider.size = v3);
				} else if (com is CapsuleCollider capsuleCollider) {
					AddBoolEditor(AddPropText("enabled"), () => capsuleCollider.enabled, b => capsuleCollider.enabled = b);
					AddBoolEditor(AddPropText("Is Trigger"), () => capsuleCollider.isTrigger, b => capsuleCollider.isTrigger = b);
					AddButton(AddPropText("Material"), () => capsuleCollider.material == null ? "None" : capsuleCollider.material.name, null);
					AddVector3Editor(AddPropText("Center"), () => capsuleCollider.center, v3 => capsuleCollider.center = v3);
					AddFloatEditor(AddPropText("Radius"), () => capsuleCollider.radius, f => capsuleCollider.radius = f);
				} else if (com is AudioSource audioSource) {
					AddBoolEditor(AddPropText("enabled"), () => audioSource.enabled, b => audioSource.enabled = b);
					AddButton(AddPropText("AudioClip"), () => audioSource.clip == null ? "None" : audioSource.clip.name, () => {
						if (audioSource.clip == null) return;
						if (mAudioSource.clip != audioSource.clip) {
							mAudioSource.clip = audioSource.clip;
							mAudioSource.Play();
						} else if (mAudioSource.isPlaying) mAudioSource.Stop();
						else mAudioSource.Play();
					});
					AddButton(AddPropText("Output"), () => audioSource.outputAudioMixerGroup == null ? "None" : audioSource.outputAudioMixerGroup.name, null);
					AddBoolEditor(AddPropText("Mute"), () => audioSource.mute, b => audioSource.mute = b);
					AddBoolEditor(AddPropText("ByPass Effects"), () => audioSource.bypassEffects, b => audioSource.bypassEffects = b);
					AddBoolEditor(AddPropText("Bypass Listener Effects"), () => audioSource.bypassListenerEffects, b => audioSource.bypassListenerEffects = b);
					AddBoolEditor(AddPropText("Bypass Reverb Zones"), () => audioSource.bypassReverbZones, b => audioSource.bypassReverbZones = b);
					AddBoolEditor(AddPropText("Play On Awake"), () => audioSource.playOnAwake, b => audioSource.playOnAwake = b);
					AddBoolEditor(AddPropText("Loop"), () => audioSource.loop, b => audioSource.loop = b);
					AddIntEditor(AddPropText("Priority"), () => audioSource.priority, i => audioSource.priority = i, 0, 256);
					AddFloatEditor(AddPropText("Volume"), () => audioSource.volume, f => audioSource.volume = f, 0, 1);
					AddFloatEditor(AddPropText("Pitch"), () => audioSource.pitch, f => audioSource.pitch = f, -3, 3);
					AddFloatEditor(AddPropText("Stereo Pan"), () => audioSource.panStereo, f => audioSource.panStereo = f, -1, 1);
					AddFloatEditor(AddPropText("Spatial Blend"), () => audioSource.spatialBlend, f => audioSource.spatialBlend = f, 0, 1);
					AddFloatEditor(AddPropText("Reverb Zone Mix"), () => audioSource.reverbZoneMix, f => audioSource.reverbZoneMix = f, 0, 1.1f);
					AddGroupText("3D Sound Settings");
					AddFloatEditor(AddPropText("Droppler Level"), () => audioSource.dopplerLevel, f => audioSource.dopplerLevel = f, 0, 5);
					AddFloatEditor(AddPropText("Spread"), () => audioSource.spread, f => audioSource.spread = f, 0, 360);
					AddEnumEditor(AddPropText("Volume Rolloff"), typeof(AudioRolloffMode), () => (int)audioSource.rolloffMode, i => audioSource.rolloffMode = (AudioRolloffMode)i);
					AddFloatEditor(AddPropText("Min Distance"), () => audioSource.minDistance, f => audioSource.minDistance = f, 0, float.PositiveInfinity);
					AddFloatEditor(AddPropText("Max Distance"), () => audioSource.maxDistance, f => audioSource.maxDistance = f, 0, float.PositiveInfinity);
				} else {
					var type = com.GetType();
					var props = type.GetProperties();

					foreach (var prop in props) {
						try {
							var name = Regex.Replace(prop.Name, "([a-z])([A-Z])", "$1 $2");
							name = name.Substring(0, 1).ToUpper() + name.Substring(1);
							var propText = AddPropText(name);
							if (prop.PropertyType == typeof(Vector3)) {
								AddVector3Editor(propText, () => (Vector3)prop.GetValue(com), v3 => prop.SetValue(com, v3));
							} else if (prop.PropertyType == typeof(bool)) {
								AddBoolEditor(propText, () => (bool)prop.GetValue(com), b => prop.SetValue(com, b));
							} else if (prop.PropertyType.IsEnum) {
								AddEnumEditor(propText, prop.PropertyType, () => (int)prop.GetValue(com), i => prop.SetValue(com, i));
							} else {
								AddButton(propText, () => { 
									try { 
										return prop.GetValue(com).ToString(); 
									} catch { }
									return "Error";
								}, null);
								//var valueEditor = new GameObject($"{name} Editor");
								//valueEditor.transform.SetParent(propText.transform);
								//var valueR = valueEditor.AddComponent<RectTransform>();
								//valueR.anchorMin = Vector2.one;
								//valueR.anchorMax = Vector2.one;
								//valueR.pivot = Vector2.one;
								//valueR.anchoredPosition = new Vector2(-20, 0);
								//valueR.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
								//valueR.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
								//var valueV = valueEditor.AddComponent<FloatEditor>();
								//valueV.size = size;
								//valueV.fontSize = fontSize;
								//valueV.component = com;
								//valueV.propInfo = prop;
								//offset -= size * 1.2f;
							}
						} catch { }
					}
				}
				y += offset;
			}

			var contentIR = contentI.GetComponent<RectTransform>();
			contentIR.anchoredPosition = Vector2.zero;
			contentIR.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
			contentIR.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -y);
			scrollViewH.SetActive(false);
			scrollViewI.SetActive(true);
		}

		public void RefreshGameObjects() {
			contentH.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -1);
			foreach (var editLine in editLines) {
				if (editLine != null) {
					editLine.SetActive(false);
				}
			}
			editGameObjects.Clear();
			string sceneName = SceneManager.GetActiveScene().name;
			foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>()) {
				if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave) {
					continue;
				}
				if (sceneName != go.gameObject.scene.name) {
					continue;
				}
				if (go.GetComponent<SceneViewerFlag>() == null && go.transform.parent == null) {
					CreateEditGameObject(go, 0, null);
				}
			}
			foreach (var obj in FindObjectsOfType<GameObject>()) {
				if (obj.GetComponent<SceneViewerFlag>() == null && obj.transform.parent == null) {
					CreateEditGameObject(obj, 0, null);
				}
			}
			contentH.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
			contentH.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size * editGameObjects.Count);
		}

		public void InitHierarchy() {
			editLines = new GameObject[Mathf.CeilToInt(Screen.height / size)];
			for (int i = 0; i < editLines.Length; i++) {
				var btn = new GameObject($"Btn{i}");
				btn.AddComponent<SceneViewerFlag>();
				btn.AddComponent<Image>();
				btn.AddComponent<Button>().onClick.AddListener(new UnityAction(() => {
					btn.GetComponent<EditLine>().SwitchOpen();
					RefreshView();
				}));
				var funBtn = new GameObject("FuncBtn");
				funBtn.AddComponent<SceneViewerFlag>();
				funBtn.AddComponent<Image>();
				funBtn.AddComponent<Button>().onClick.AddListener(new UnityAction(() => {
					inspectorObj = btn.GetComponent<EditLine>().editGameObject.gameObject;
					LoadInspector();
				}));
				var label = new GameObject("Label");
				label.AddComponent<SceneViewerFlag>();
				label.transform.SetParent(btn.transform);
				label.AddComponent<Text>();
				label.GetComponent<Text>().font = font;
				label.GetComponent<Text>().fontSize = (int)(size * 0.7f);
				label.GetComponent<Text>().color = Color.black;
				label.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
				label.GetComponent<Text>().text = "＋";
				label.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
				label.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
				label.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				var text = new GameObject("Text");
				text.AddComponent<SceneViewerFlag>();
				text.transform.SetParent(btn.transform);
				text.AddComponent<Text>();
				text.GetComponent<Text>().font = font;
				text.GetComponent<Text>().fontSize = (int)(size * 0.5f);
				text.GetComponent<Text>().color = Color.black;
				text.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
				text.GetComponent<Text>().text = gameObject.name;
				text.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
				btn.transform.SetParent(contentH.transform);
				btn.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
				btn.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
				btn.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
				btn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
				btn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
				funBtn.transform.SetParent(btn.transform);
				funBtn.GetComponent<Image>().color = new Color(1, 1, 1, 0.2f);
				funBtn.GetComponent<RectTransform>().anchorMin = text.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0.5f);
				funBtn.GetComponent<RectTransform>().anchorMax = text.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.5f);
				funBtn.GetComponent<RectTransform>().pivot = text.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
				text.GetComponent<RectTransform>().anchoredPosition = new Vector2(size * 0.3f, 0);
				funBtn.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				text.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
				funBtn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
				btn.AddComponent<EditLine>().Init(funBtn.GetComponent<Button>(), text.GetComponent<Text>(), label.GetComponent<Text>());
				editLines[i] = btn;
			}
		}

		private void Update() {
			if (find && Input.GetMouseButtonDown(0)) {
				find = false;
				foreach (var edit in editGameObjects) {
					edit.highlight = false;
				}
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out RaycastHit hit)) {
					foreach (var edit in editGameObjects) {
						if (edit.gameObject == hit.transform.gameObject) {
							var e = edit;
							do {
								e.open = true;
								e.highlight = true;
								e = e.father;
							} while (e != null);
							break;
						}
					}
				}
				scrollViewH.SetActive(true);
				RefreshView();
			}
		}

		public EditGameObject CreateEditGameObject(GameObject gameObject, int level, EditGameObject father) {
			var editGameObject = new EditGameObject(gameObject, level, father);
			int index = editGameObjects.IndexOf(editGameObject);
			if (index != -1) {
				editGameObjects[index] = editGameObject;
			} else {
				editGameObjects.Add(editGameObject);
			}
			foreach (Transform child in gameObject.transform) {
				editGameObject.children.Add(CreateEditGameObject(child.gameObject, level + 1, editGameObject));
			}
			return editGameObject;
		}
	}
}