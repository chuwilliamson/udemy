using UnityEngine;

namespace Udemy
{
    [AddComponentMenu("Udemy/DNABehaviour", 0), DisallowMultipleComponent,
     RequireComponent(typeof(SpriteRenderer))]
    public class DNABehaviour : MonoBehaviour
    {
        [SerializeField] private Chromosome _chromosome;
        private SpriteRenderer _spriteRenderer;

        public delegate void ChromosomeCallback(Chromosome chromosome);

        public void Init(Chromosome chromosome, ChromosomeCallback onSuccessCreate)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _chromosome = chromosome;
            _chromosome.OnChromosomeChanged += c => _spriteRenderer.color = c.GeneColor;
            _chromosome.OnChromosomeDied += c => Destroy(gameObject);
            
            _chromosome.GeneColor = new Color(transform.position.x, transform.position.y, transform.position.z);
            
            onSuccessCreate(_chromosome);
        }

        public void Kill()
        {
            _chromosome.IsDead = true;
        }
    }
}