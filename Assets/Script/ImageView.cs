
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

public class ImageView : MonoBehaviour, IView
{
	/// <summary>
	/// 이미지 패널
	/// </summary>
	[SerializeField]
	private Image imagePanel;

	/// <summary>
	/// 로드된 이미지 리스트
	/// </summary>
	private readonly Dictionary<int, Sprite> loadedImageList = new Dictionary<int, Sprite>();

	/// <summary>
	/// 따로 지정하지 않으면 자동으로 배정되는 이미지 인덱스
	/// </summary>
	private int loadedCount = 0;

	/// <summary>
	/// 현재 보여지고 있는 이미지의 인덱스
	/// </summary>
	private int currentCount = 0;

	public void ShowView(bool show)
	{
		loadedCount = 0;
		currentCount = 0;

		this.gameObject.SetActive(show);

		if (!show)
		{
			Dispose();
		}
	}

	/// <summary>
	/// 이미지를 선택해서 띄운다
	/// 사용법: <see cref="http://wiki.unity3d.com/index.php/FileBrowser"/>
	/// </summary>
	public void SelectImage()
	{
		// 탐색기 열기
		FileBrowser.ShowLoadDialog(path => { ShowImage(path); }, null,
			initialPath: Application.streamingAssetsPath + "/Images/", title: "Select Image");
	}

	/// <summary>
	/// 이미지를 선택해서 업로드
	/// </summary>
	public void UploadImage()
	{
		FileBrowser.ShowLoadDialog(path => { UploadImage(path); }, null,
			initialPath: Application.streamingAssetsPath + "/Images/", title: "Select Image");
	}

	/// <summary>
	/// 다음 이미지 열기
	/// </summary>
	public void NextImage()
	{
		if (!loadedImageList.TryGetValue(unchecked(currentCount + 1), out var image)) return;

		currentCount = unchecked(currentCount + 1);

		imagePanel.sprite = image;
	}

	/// <summary>
	/// 이전 이미지 열기
	/// </summary>
	public void PrevImage()
	{
		if (!loadedImageList.TryGetValue(unchecked(currentCount - 1), out var image)) return;

		currentCount = unchecked(currentCount - 1);

		imagePanel.sprite = image;
	}

	private void ShowImage(string path)
	{
		currentCount = unchecked(loadedCount++);

		var assetPath = Path.Combine(Application.streamingAssetsPath + "/Images/", Path.GetFileName(path));

		var imageByte = File.ReadAllBytes(assetPath);

		var tex = new Texture2D(2, 2);
		tex.LoadImage(imageByte);

		var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
		imagePanel.sprite = sprite;

		loadedImageList.Add(currentCount, sprite);
	}

	private void UploadImage(string path)
	{
		// TODO: 업로드 루틴
	}

	/// <summary>
	/// 리소스 정리
	/// </summary>
	private void Dispose()
	{
		loadedCount = 0;
		currentCount = 0;

		imagePanel.sprite = null;

		loadedImageList.Clear();
	}
}
