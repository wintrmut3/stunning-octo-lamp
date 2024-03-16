using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using UnityEditor.Tilemaps;
using DG.Tweening;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private List<Tuple<int, int>> timingList;
    public GameObject circlePrefab;
    private int songStartTime;
    List<Tuple<int,int>> ParseSong()
    {
        List<Tuple<int, int>> ret = new List<Tuple<int, int>>();
        print("parsing song");
        string filePath = "Assets/Yorushika_Toumin.osu";
        int lineIdx = 0;

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found.");
            return null;
        }

        // Read the file line by line
        using (StreamReader sr = new StreamReader(filePath))
        {
            string line;
            bool foundHitObjects = false;
            // Read until the end of the file or until we find "[HitObjects]"
            while ((line = sr.ReadLine()) != null)
            {
                lineIdx++;
                // if(lineIdx%100==0) Debug.Log($"@ line   {lineIdx}");
                if (line.Contains("[HitObjects]"))
                {
                    foundHitObjects = true;

                    Debug.Log("Found [HitObjects] at line: " + line);
                    break;
                }
            }
            // If "[HitObjects]" was not found
            if (!foundHitObjects)
            {
                Console.WriteLine("[HitObjects] not found in the file.");
            }
            
            print($"Starting search at {lineIdx}");
            sr.ReadLine(); // consume one extra line
            while ((line = sr.ReadLine()) != null)
            {
                String[] sline = line.Split(',');
                int time_millis = int.Parse(sline[2]);
                int hit_sound = int.Parse(sline[4]);

                if (hit_sound == 0 || hit_sound ==1)
                {
                    // impact
                    print($"Adding to ret: {time_millis}, {hit_sound}");
                    // avoid adding duplicate sounds
                    if(ret.Any() && ret[ret.Count-1].Item1 != time_millis)
                        ret.Add(new Tuple<int, int>(time_millis, hit_sound));
                    if (!ret.Any())  ret.Add(new Tuple<int, int>(time_millis, hit_sound));
                }


            }

        }

        return ret;


    }

    // Start is called before the first frame update
    void Start()
    {
        timingList = ParseSong();
        print(timingList);
        currentTimeMillis = songStartTime;
    }
    
    int array_idx = 0; // to reference timing array

    public float forceAmt;
    public float innerRadius, targetRadius;
    public float lifetime;
    private int currentTimeMillis;

    [FormerlySerializedAs("ms_offset_ring")] public float msOffset;
    // Update is called once per frame
    private List<int> target_timings = new List<int>();
    void Update()
    {
        // note - song start time is rounded to seconds, which is not a problem if it's 0 but otherwise will cause
        // desync of effects by a constant offset (+/- .5s in either direction)
        
        currentTimeMillis += (int)(Time.deltaTime*1000);
        Debug.Log($"Next entry: {timingList[array_idx]} | Current Time {currentTimeMillis}");

        if (timingList[array_idx].Item1 <= currentTimeMillis+msOffset)
        {
            int target_timing_ms = timingList[array_idx].Item1;
            print("hit timing");
            array_idx++;
            var new_circle = Instantiate(circlePrefab);
            float ng = (array_idx % 8) / 8.0f * 2*Mathf.PI;
            
            new_circle.transform.position = new Vector3(Mathf.Sin(ng), Mathf.Cos(ng), 0) * innerRadius;
            // new_circle.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Sin(ng), Mathf.Cos(ng))*forceAmt, ForceMode2D.Impulse);
            new_circle.transform.DOMove(new Vector3(Mathf.Sin(ng), Mathf.Cos(ng), 0) * targetRadius, lifetime).SetEase(Ease.Linear);
            new_circle.transform.DOScale(Vector3.one * 0.5f, msOffset/1000f).From(Vector3.zero).SetEase(Ease.OutBack
            );
            new_circle.transform.DOScale(Vector3.zero, msOffset/2000f).SetDelay(msOffset/1000f).SetEase(Ease.OutExpo);
            target_timings.Add(timingList[array_idx].Item1);
            new_circle.GetComponent<SpriteRenderer>().DOColor(Color.red, 0.1f).From(Color.white).SetDelay(msOffset/1000-100/1000f);
            new_circle.GetComponent<SpriteRenderer>().DOColor(Color.clear, 0.1f).SetDelay(msOffset/1000 + 50/1000f);
            
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // if (Mathf.Abs(currentTimeMillis - target_timings))
        }
    }
}