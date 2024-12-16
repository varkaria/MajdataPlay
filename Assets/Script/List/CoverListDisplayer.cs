using MajdataPlay.Collections;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.List
{
    public class CoverListDisplayer : MonoBehaviour
    {
        public bool IsDirList => Mode == CoverListMode.Directory;
        public bool IsChartList => Mode == CoverListMode.Chart;
        public CoverListMode Mode { get; set; } = CoverListMode.Directory;

        
        public string soundEffectName;
        public GameObject CoverSmallPrefab;
        public GameObject DirSmallPrefab;
        public CoverBigDisplayer CoverBigDisplayer;
        public SubInfoDisplayer SubInfoDisplayer;

        public int desiredListPos = 0;
        public float listPosReal;
        public float turnSpeed;
        public float radius;
        public float offset;

        public int selectedDifficulty = 0;

        private int coveri = 0;

        SongCollection[] _dirs = Array.Empty<SongCollection>();
        SongCollection _songs = new SongCollection();
        CoverSmallDisplayer[] _dirCovers = new CoverSmallDisplayer[16];
        CoverSmallDisplayer[] _chartCovers = new CoverSmallDisplayer[16];
        List<GameObject> covers = new List<GameObject>();

        void Awake()
        {
            for (var i = 0; i < 16; i++)
            {
                var dirCover = Instantiate(DirSmallPrefab, transform);
                var chartCover = Instantiate(CoverSmallPrefab, transform);
                var dirComponent = dirCover.GetComponent<CoverSmallDisplayer>();
                var chartComponent = chartCover.GetComponent<CoverSmallDisplayer>();

                _dirCovers[i] = dirComponent;
                _chartCovers[i] = chartComponent;
                dirComponent.SetActive(false);
                chartComponent.SetActive(false);
            }
        }
        public void SwitchToDirList(SongCollection[] _dirs)
        {
            foreach (var cover in covers)
            {
                Destroy(cover);
            }
            covers.Clear();
            Mode = CoverListMode.Directory;
            this._dirs = _dirs;
            desiredListPos = SongStorage.CollectionIndex;
            foreach (var dir in _dirs)
            {
                var obj = Instantiate(DirSmallPrefab, transform);
                var coversmall = obj.GetComponent<CoverSmallDisplayer>();
                //coversmall.SetCover(song.SongCover);
                coversmall.SetLevelText(dir.Name);
                covers.Add(obj);
                obj.SetActive(false);
            }
            if (desiredListPos > covers.Count) 
                desiredListPos = 0;
            listPosReal = desiredListPos;
            SlideToList(desiredListPos);
        }

        public void SwitchToSongList()
        {
            if (_songs.Count == 0) return;
            foreach (var cover in covers)
            {
                Destroy(cover);
            }
            covers.Clear();
            Mode = CoverListMode.Chart;
            desiredListPos = SongStorage.WorkingCollection.Index;
            foreach (var song in _songs)
            {
                var obj = Instantiate(CoverSmallPrefab, transform);
                var coversmall = obj.GetComponent<CoverSmallDisplayer>();
                coversmall.SetOpacity(0f);
                coversmall.SetCover(song);
                coversmall.SetLevelText(song.Levels[selectedDifficulty]);
                covers.Add(obj);
                obj.SetActive(false);
            }
            if (desiredListPos > covers.Count) 
                desiredListPos = 0;
            listPosReal = desiredListPos;
            SlideToList(desiredListPos);
        }


        public void SlideDifficulty(int delta)
        {
            selectedDifficulty += delta;
            SlideToDifficulty(selectedDifficulty);
        }

        public void SlideToDifficulty(int pos)
        {
            selectedDifficulty = pos;
            if (selectedDifficulty > 6)
            {
                selectedDifficulty = 0;
            }
            if (selectedDifficulty < 0)
            {
                selectedDifficulty = 6;
            }
            MajInstances.GameManager.SelectedDiff = (ChartLevel)selectedDifficulty;
            CoverBigDisplayer.SetDifficulty(selectedDifficulty);
            if (IsChartList)
            {
                var songinfo = _songs[desiredListPos];
                var songScore = MajInstances.ScoreManager.GetScore(songinfo, MajInstances.GameManager.SelectedDiff);
                CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
                CoverBigDisplayer.SetScore(songScore);

                for (int i = 0; i < covers.Count; i++)
                {
                    var text = _songs[i].Levels[selectedDifficulty];
                    if (text == null || text == "") text = "-";
                    covers[i].GetComponent<CoverSmallDisplayer>().SetLevelText(text);
                }
            }
        }

        public void SlideList(int delta)
        {
            switch(Mode)
            {
                case CoverListMode.Directory:
                    SongStorage.CollectionIndex += delta;
                    desiredListPos = SongStorage.CollectionIndex;
                    break;
                case CoverListMode.Chart:
                    var collection = SongStorage.WorkingCollection;
                    collection.Move(delta);
                    desiredListPos = collection.Index;
                    break;
            }
            SlideToList(desiredListPos);
        }
        public void RefreshList()
        {
            var collection = SongStorage.WorkingCollection;
            desiredListPos = collection.Index;
            SlideToList(desiredListPos);
        }

        public void SlideToList(int pos)
        {
            MajInstances.AudioManager.PlaySFX("tap_perfect.wav");
            desiredListPos = pos;
            if (desiredListPos >= covers.Count)
            {
                desiredListPos = covers.Count - 1;
            }
            if (desiredListPos <= 0)
            {
                desiredListPos = 0;
            }
            switch(Mode)
            {
                case CoverListMode.Directory:
                    _songs = _dirs[desiredListPos];
                    CoverBigDisplayer.SetMeta(_songs.Name, "", "", "");
                    CoverBigDisplayer.SetScore(new MaiScore());
                    SongStorage.CollectionIndex = desiredListPos;
                    break;
                case CoverListMode.Chart:
                    var songinfo = _songs[desiredListPos];
                    var songScore = MajInstances.ScoreManager.GetScore(songinfo, MajInstances.GameManager.SelectedDiff);
                    CoverBigDisplayer.SetCover(songinfo);
                    CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
                    CoverBigDisplayer.SetScore(songScore);
                    SubInfoDisplayer.RefreshContent(songinfo);
                    GetComponent<PreviewSoundPlayer>().PlayPreviewSound(songinfo);
                    SongStorage.WorkingCollection.Index = desiredListPos;
                    break;
            }
        }
        private void FixedUpdate()
        {
            var delta = (desiredListPos - listPosReal) * turnSpeed;
            listPosReal += Mathf.Clamp(delta, -1f, 1f);
            if (Mathf.Abs(desiredListPos - listPosReal) < 0.01f) 
                listPosReal = desiredListPos;
            for (int i = 0; i < covers.Count; i++)
            {
                var distance = i - listPosReal;
                if (Mathf.Abs(distance) > 7)
                {
                    covers[i].SetActive(false);
                    continue;
                }
                covers[i].SetActive(true);
                covers[i].GetComponent<RectTransform>().anchoredPosition = GetCoverPosition(radius, distance * Mathf.Deg2Rad * 22.5f);
                var scd = covers[i].GetComponent<CoverSmallDisplayer>();
                if (Mathf.Abs(distance) > 6)
                {
                    scd.SetOpacity(-Mathf.Abs(distance) + 7);
                }
                else
                {
                    scd.SetOpacity(1f);
                }
            }
            if (IsDirList && Time.frameCount % 50 == 0)
            {
                if (_songs.Count > 0)
                {
                    if (coveri >= _songs.Count) coveri = 0;
                    CoverBigDisplayer.SetCover(_songs[coveri++]);
                }
                else
                {
                    CoverBigDisplayer.SetNoCover();
                }
            }
        }
        

        Vector3 GetCoverPosition(float radius, float position)
        {
            return new Vector3(radius * Mathf.Sin(position), radius * Mathf.Cos(position));
        }
    }
}