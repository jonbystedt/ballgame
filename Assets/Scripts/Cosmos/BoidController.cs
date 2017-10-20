using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace ProceduralToolkit.Examples
{
    public class Boid
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 cohesion;
        public Vector3 separation;
        public Vector3 alignment;
    }

    /// <summary>
    /// A single-mesh particle system with birds-like behaviour 
    /// </summary>
    /// <remarks>
    /// http://en.wikipedia.org/wiki/Boids
    /// </remarks>
    public class BoidController
    {
        public Vector3 anchor = Vector3.zero;
        public float innerSphere = 30;
        public float worldSphere = 10;
        public float terrainCheckSphere = 30;

        public int swarmCount = 500;
        public int maxSpeed = 5;
        public float interactionRadius = 5;
        public float cohesionCoefficient = 1;
        public float separationDistance = 3;
        public float separationCoefficient = 10;
        public float alignmentCoefficient = 5;

        /// <summary>
        /// Number of neighbours participating in calculations
        /// </summary>
        public int maxBoids = 5;
        /// <summary>
        /// Percentage of swarm simulated in each frame
        /// </summary>
        public float simulationPercent = 0.01f;
        public float flutterBoost = 100f;

        private List<Boid> boids = new List<Boid>();
        private MeshDraft template;
        private MeshDraft draft;
        private Mesh mesh;
        private List<Boid> neighbours = new List<Boid>();
        private int maxSimulationSteps;
        private Transform transform;
        private bool reverse = false;

        /// <summary>
        /// Generate new colors and positions for boids
        /// </summary>
        public Mesh Generate(Color colorA, Color colorB)
        {
            template = MeshDraft.Tetrahedron(0.3f);

            // Avoid vertex count overflow
            swarmCount = Mathf.Min(65000/template.vertices.Count, swarmCount);
            // Optimization trick: in each frame we simulate only small percent of all boids
            maxSimulationSteps = Mathf.RoundToInt(swarmCount*simulationPercent);
            // int vertexCount = swarmCount*template.vertices.Count;

            // Paint template in random color
            template.colors.Clear();
            // Assuming that we are dealing with tetrahedron, first vertex should be boid's "nose"
            template.colors.Add(colorA);
            for (int i = 1; i < template.vertices.Count; i++)
            {
                template.colors.Add(Tile.Lighten(colorB, i * 0.1f));
            }

            draft = new MeshDraft
            {
                name = "Boids",
                vertices = new List<Vector3>(),
                triangles = new List<int>(),
                normals = new List<Vector3>(),
                uv = new List<Vector2>(),
                colors = new List<Color>()
            };

            for (var i = 0; i < swarmCount; i++)
            {
                // Assign random starting values for each boid
                var boid = new Boid
                {
                    position = Random.onUnitSphere*worldSphere,
                    rotation = Random.rotation,
                    velocity = Random.onUnitSphere*maxSpeed
                };
                boid.position.y = Mathf.Abs(boid.position.y);
                boid.velocity.y = Mathf.Abs(boid.velocity.y);
                boids.Add(boid);

                draft.Add(template);
            }

            mesh = draft.ToMesh();
            mesh.MarkDynamic();

            return mesh;
        }

        /// <summary>
        /// Run simulation
        /// </summary>
        public IEnumerator CalculateVelocities(Transform t)
        {
            if (transform == null)
            {
                transform = t;
            }

            int simulationStep = 0;

            //Game.Log(worldPos.x.ToString() + "   " + worldPos.y.ToString() + "   " + worldPos.z.ToString());

            for (int currentIndex = 0; currentIndex < boids.Count; currentIndex++)
            {
                // Optimization trick: in each frame we simulate only small percent of all boids
                simulationStep++;
                if (simulationStep > maxSimulationSteps)
                {
                    simulationStep = 0;
                    yield return null;
                }

                var boid = boids[currentIndex];


                // Search for nearest neighbours
                neighbours.Clear();
                for (int i = 0; i < boids.Count; i++)
                {
                    Boid neighbour = boids[i];

                    Vector3 toNeighbour = neighbour.position - boid.position;
                    if (toNeighbour.sqrMagnitude < interactionRadius)
                    {
                        neighbours.Add(neighbour);
                        if (neighbours.Count == maxBoids)
                        {
                            break;
                        }
                    }
                }

                if (neighbours.Count < 2) continue;

                boid.velocity = Vector3.zero;
                boid.cohesion = Vector3.zero;
                boid.separation = Vector3.zero;
                boid.alignment = Vector3.zero;

                // Calculate boid parameters
                int separationCount = 0;
                for (int i = 0; i < neighbours.Count && i < maxBoids; i++)
                {
                    Boid neighbour = neighbours[i];

                    boid.cohesion += neighbour.position;
                    boid.alignment += neighbour.velocity;

                    Vector3 toNeighbour = neighbour.position - boid.position;
                    if (toNeighbour.sqrMagnitude > 0 &&
                        toNeighbour.sqrMagnitude < separationDistance*separationDistance)
                    {
                        boid.separation += toNeighbour/toNeighbour.sqrMagnitude;
                        separationCount++;
                    }
                }

                // Clamp all parameters to safe values
                boid.cohesion /= Mathf.Min(neighbours.Count, maxBoids);
                boid.cohesion = Vector3.ClampMagnitude(boid.cohesion - boid.position, maxSpeed);
                boid.cohesion *= cohesionCoefficient;

                if (separationCount > 0)
                {
                    boid.separation /= separationCount;
                    boid.separation = Vector3.ClampMagnitude(boid.separation, maxSpeed);
                    boid.separation *= separationCoefficient;
                }

                boid.alignment /= Mathf.Min(neighbours.Count, maxBoids);
                boid.alignment = Vector3.ClampMagnitude(boid.alignment, maxSpeed);
                boid.alignment *= alignmentCoefficient;

                // Calculate resulting velocity
                Vector3 velocity = boid.cohesion + boid.separation + boid.alignment;
                boid.velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

                if (boid.velocity == Vector3.zero)
                {
                    // Prevent boids from stopping
                    boid.velocity = Random.onUnitSphere*maxSpeed;
                    boid.velocity.y = Mathf.Abs(boid.velocity.y);
                }  

                AvoidTerrain(boid);   
            }
        }

        void AvoidTerrain(Boid boid)
        {
            if (transform == null)
            {
                return;
            }
            float s = 0.75f;
            int hits = 0;
            bool directHit = false;
            for (float x = -s; x <= s; x = x + s)
            {
                for (float y = -s; y <= s; y = y + s)
                {
                    for (float z = -s; z <= s; z = z + s)
                    {
                        Vector3 p = new Vector3(boid.position.x + x, boid.position.y + y, boid.position.z + z);
                        Vector3 pos = p + boid.velocity*Time.deltaTime*2f;
                        WorldPosition worldPos = new WorldPosition(transform.TransformPoint(pos));
                        ushort block = World.GetBlock(worldPos);
                        if ((block != Block.Air && block != Block.Null && worldPos.y < 16) || worldPos.y < -48)
                        {
                            hits++;
                            if (x == 0 && y == 0 && z == 0)
                            {
                                directHit = true;
                            }
                        }
                        
                    }
                }
            }
            for (float x = -s; x <= s; x = x + s)
            {
                for (float y = -s; y <= s; y = y + s)
                {
                    for (float z = -s; z <= s; z = z + s)
                    {
                        Vector3 p = new Vector3(boid.position.x + x, boid.position.y + y, boid.position.z + z);
                        Vector3 pos = p + boid.velocity;
                        WorldPosition worldPos = new WorldPosition(transform.TransformPoint(pos));
                        ushort block = World.GetBlock(worldPos);
                        if ((block != Block.Air && block != Block.Null && worldPos.y < 16) || worldPos.y < -48)
                        {
                            hits++;
                            if (x == 0 && y == 0 && z == 0)
                            {
                                directHit = true;
                            }
                        }
                        
                    }
                }
            }

            if (hits == 0)
            {
                boid.rotation = Quaternion.FromToRotation(Vector3.up, boid.velocity);
            }
            else
            {
                boid.rotation = Quaternion.FromToRotation(Vector3.up, Vector3.RotateTowards(boid.velocity, -boid.velocity, Time.deltaTime *  hits, 0f));
            }
            if (directHit)
            {
                boid.velocity -= Vector3.up * 100f;
                boid.velocity = Vector3.ClampMagnitude(boid.velocity, maxSpeed);
            }
            if (hits > 0)
            {
                boid.velocity = Vector3.RotateTowards(boid.velocity, -boid.velocity, Time.deltaTime * hits * 5f, 0f);
                boid.velocity *= flutterBoost;
                boid.velocity += Vector3.up;
                boid.velocity = Vector3.ClampMagnitude(boid.velocity, maxSpeed);
            }  
        }

        /// <summary>
        /// Apply simulation to mesh
        /// </summary>
        public void Update()
        {
            if (!transform)
            {
                return;
            }

            interactionRadius = (float)TerrainGenerator.GetNoise1D(new Vector3(Cosmos.CurrentTime,0,0), NoiseConfig.boidInteraction, NoiseType.Value) + 10f;
            alignmentCoefficient = (float)TerrainGenerator.GetNoise1D(new Vector3(Cosmos.CurrentTime,0,0), NoiseConfig.boidAlignment, NoiseType.Value) + 20f;
            separationDistance  = (float)TerrainGenerator.GetNoise1D(new Vector3(Cosmos.CurrentTime,0,0), NoiseConfig.boidDistance, NoiseType.Value) + 5f;

            for (int i = 0; i < boids.Count; i++)
            {
                var boid = boids[i];

                // Contain boids in sphere
                Vector3 distanceToAnchor = anchor - boid.position;
                if (distanceToAnchor.sqrMagnitude > worldSphere*worldSphere)
                {
                    boid.velocity += distanceToAnchor/worldSphere;
                    boid.velocity = Vector3.ClampMagnitude(boid.velocity, maxSpeed);
                }

                if (distanceToAnchor.sqrMagnitude < innerSphere*innerSphere)
                {
                    boid.velocity -= distanceToAnchor/worldSphere;
                }

                else if (distanceToAnchor.sqrMagnitude < terrainCheckSphere*terrainCheckSphere)
                {
                    AvoidTerrain(boid);
                }

                boid.position += boid.velocity * Time.deltaTime;
                        
                SetBoidVertices(boid, i);
            }
            mesh.SetVertices(draft.vertices);
            mesh.RecalculateNormals();
        }

        private void SetBoidVertices(Boid boid, int index)
        {
            for (int i = 0; i < template.vertices.Count; i++)
            {
                draft.vertices[index*template.vertices.Count + i] = boid.rotation*template.vertices[i] + boid.position;
            }
        }
    }
}