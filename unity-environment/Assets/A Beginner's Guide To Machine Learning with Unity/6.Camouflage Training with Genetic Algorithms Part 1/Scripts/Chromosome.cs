using UnityEngine;

namespace Udemy
{
    //they will impart there color onto the next generation
    //gene for colour
    //you also want to store for each person whether or not they have been clicked
    //set this to true after you click on them
    //when they die we will store how long they lived for

    [CreateAssetMenu(menuName = "Udemy/Chromosome")]
    public class Chromosome : ScriptableObject
    {
        [SerializeField] private Color _geneColor;
        [SerializeField] private bool _isDead;
        public float TimeToDie;
        public Color GeneColor
        {
            get { return _geneColor; }
            set
            {
                _geneColor = value;
                OnChromosomeChanged?.Invoke(this);
            }
        }


        public bool IsDead
        {
            get { return _isDead; }
            set
            {
                _isDead = value;
                OnChromosomeDied?.Invoke(this);
            }
        }

        

        public delegate void OnValueChanged(Chromosome chromosome);
        public OnValueChanged OnChromosomeChanged;
        public OnValueChanged OnChromosomeDied;
    }
}