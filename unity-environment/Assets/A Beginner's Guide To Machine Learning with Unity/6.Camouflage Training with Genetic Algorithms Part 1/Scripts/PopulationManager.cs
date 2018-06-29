using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Udemy
{
    public class PopulationManager : MonoBehaviour
    {
        public GameObject PersonPrefab;
        public int PopulationSize;
        public List<Chromosome> Population;

        private float _elapsed;

        private GUIStyle _guiStyle;

        private int _generation = 1;
        private int _deathCount;

        private void OnGUI()
        {
            _guiStyle = new GUIStyle() {fontSize = 25, normal = new GUIStyleState {textColor = Color.white}};
            GUI.Label(new Rect(10, 10, 100, 20), string.Format("Generation: {0}", _generation.ToString()), _guiStyle);
            var elapsed = (int) _elapsed;
            GUI.Label(new Rect(10, 65, 100, 20), string.Format("Trial Time: {0}", elapsed.ToString()), _guiStyle);
        }

        private void Start()
        {
            Population = CreatePopulation(PersonPrefab, null); 

        }

        private List<Chromosome> CreatePopulation(GameObject prefab, IReadOnlyList<Chromosome> oldPopulation)
        {
            var newpopulation = new List<Chromosome>();
            for (var i = 0; i < PopulationSize; i++)
            {
                var x = Random.Range(-9f, 9f);
                var y = Random.Range(-4.5f, 4.5f);
                var pos = new Vector3(x, y, 0);
                var go = Instantiate(prefab, pos, Quaternion.identity);
                var dna = go.GetComponent<DNABehaviour>();

                var chromosome = oldPopulation == null
                    ? ScriptableObject.CreateInstance<Chromosome>()
                    : oldPopulation[i];

                chromosome.name = "chromosome " + _generation;
                dna.Init(chromosome, c => newpopulation.Add(c));
                chromosome.OnChromosomeDied += (c) =>
                {
                    c.TimeToDie = _elapsed;
                    _deathCount++;
                };
            }

            return newpopulation;
        }

        private List<Chromosome> BreedPopulation(IEnumerable<Chromosome> oldpopulation)
        {
            var newPopulation = new List<Chromosome>();
            var ordered = oldpopulation.OrderBy(o => o.TimeToDie).ToList();

            for (var i = (ordered.Count / 2); i >= 0; i--)
            {
                var mom = ordered[i];
                var dad = ordered[i + 1];
                newPopulation.Add(Breed(mom, dad));
                newPopulation.Add(Breed(dad, mom));
            }

            return newPopulation;
        }

        private static Chromosome Breed(Chromosome mom, Chromosome dad)
        {
            var chromosome = ScriptableObject.CreateInstance<Chromosome>();
            var r = Random.Range(0, 10) < 5 ? mom.GeneColor.r : dad.GeneColor.r;
            var g = Random.Range(0, 10) < 5 ? mom.GeneColor.g : dad.GeneColor.g;
            var b = Random.Range(0, 10) < 5 ? mom.GeneColor.b : dad.GeneColor.b;
            chromosome.GeneColor = new Color(r, g, b);
            return chromosome;
        }
 
        private void Update()
        {
            _elapsed += Time.deltaTime;

            if(_deathCount >= Population.Count)
            {
                Population = CreatePopulation(PersonPrefab, BreedPopulation(Population));
                _deathCount = 0;
                _elapsed = 0;
                _generation++;
            }
            
            var result = Population.FindAll(c => !c.IsDead);
            var random = Random.Range(0, result.Count - 1);
            result[random].IsDead = true;
        }
    }
}

