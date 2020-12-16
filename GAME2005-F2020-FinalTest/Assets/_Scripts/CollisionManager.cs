using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollisionManager : MonoBehaviour
{
    public CubeBehaviour[] cubes;
    public BulletBehaviour[] bullets;

    private static Vector3[] faces;

    // Start is called before the first frame update
    void Start()
    {
        cubes = FindObjectsOfType<CubeBehaviour>();

        faces = new Vector3[]
        {
            Vector3.left, Vector3.right,
            Vector3.down, Vector3.up,
            Vector3.back , Vector3.forward
        };
    }

    // Update is called once per frame
    void Update()
    {
        bullets = FindObjectsOfType<BulletBehaviour>();

        // check each AABB with every other AABB in the scene
        for (int i = 0; i < cubes.Length; i++)
        {
            for (int j = 0; j < cubes.Length; j++)
            {
                if (i != j)
                {
                    CheckAABBs(cubes[i], cubes[j]);
                }
            }
        }

        // Check each sphere against each AABB in the scene
        foreach (var bullet in bullets)
        {
            foreach (var cube in cubes)
            {
                if (cube.name != "Player")
                {
                    CheckBulletAABB(bullet, cube);
                }
                
            }
        }


    }

    public static void CheckBulletAABB(BulletBehaviour b, CubeBehaviour c)
    {
         Contact contactB = new Contact(c);

        if ((b.min.x <= c.max.x && b.max.x >= c.min.x) &&
            (b.min.y <= c.max.y && b.max.y >= c.min.y) &&
            (b.min.z <= c.max.z && b.max.z >= c.min.z))
        {
            // determine the distances between the contact extents
            float[] distances = {
                (c.max.x - b.min.x),
                (b.max.x - c.min.x),
                (c.max.y - b.min.y),
                (b.max.y - c.min.y),
                (c.max.z - b.min.z),
                (b.max.z - c.min.z)
            };

            float penetration = float.MaxValue;
            Vector3 face = Vector3.zero;

            // check each face to see if it is the one that connected
            for (int i = 0; i < 6; i++)
            {
                if (distances[i] < penetration)
                {
                    // determine the penetration distance
                    penetration = distances[i];
                    face = faces[i];
                }
            }
            
            // set the contact properties

            b.collisionNormal = face;
            b.penetration = penetration;
            //s.isColliding = true;

            
            Reflect(b);
        }

    }
    
    // This helper function reflects the bullet when it hits an AABB face
    private static void Reflect(BulletBehaviour b)
    {
        if ((b.collisionNormal == Vector3.forward) || (b.collisionNormal == Vector3.back))
        {
            if(b.collisionNormal == Vector3.forward)
            {
                b.transform.position = new Vector3( b.transform.position.x,  b.transform.position.y,  b.transform.position.z - b.penetration - 0.1f);
            }
            else
            {
                 b.transform.position = new Vector3( b.transform.position.x,  b.transform.position.y,  b.transform.position.z + b.penetration + 0.1f);
            }
            b.direction = new Vector3(b.direction.x, b.direction.y, -b.direction.z);
            Debug.Log("Reflecting in Z");
        }
        else if ((b.collisionNormal == Vector3.right) || (b.collisionNormal == Vector3.left))
        {
            if(b.collisionNormal == Vector3.forward)
            {
                b.transform.position = new Vector3( b.transform.position.x - b.penetration - 0.1f,  b.transform.position.y,  b.transform.position.z );
            }
            else
            {
                 b.transform.position = new Vector3( b.transform.position.x + b.penetration + 0.1f,  b.transform.position.y,  b.transform.position.z);
            }
            b.direction = new Vector3(-b.direction.x, b.direction.y, b.direction.z);
            Debug.Log("Reflecting in X");
        }
        else if ((b.collisionNormal == Vector3.up) || (b.collisionNormal == Vector3.down))
        {
            if(b.collisionNormal == Vector3.up)
            {
                b.transform.position = new Vector3( b.transform.position.x,  b.transform.position.y - b.penetration - 0.1f,  b.transform.position.z );
            }
            else
            {
                 b.transform.position = new Vector3( b.transform.position.x,  b.transform.position.y + b.penetration + 0.1f,  b.transform.position.z);
            }
            b.direction = new Vector3(b.direction.x, -b.direction.y, b.direction.z);
            Debug.Log("Reflecting in Y"+ b.penetration);
        }
    }


    public static void CheckAABBs(CubeBehaviour a, CubeBehaviour b)
    {
        Contact contactB = new Contact(b);

        if ((a.min.x <= b.max.x && a.max.x >= b.min.x) &&
            (a.min.y <= b.max.y && a.max.y >= b.min.y) &&
            (a.min.z <= b.max.z && a.max.z >= b.min.z))
        {
            // determine the distances between the contact extents
            float[] distances = {
                (b.max.x - a.min.x),
                (a.max.x - b.min.x),
                (b.max.y - a.min.y),
                (a.max.y - b.min.y),
                (b.max.z - a.min.z),
                (a.max.z - b.min.z)
            };

            float penetration = float.MaxValue;
            Vector3 face = Vector3.zero;

            // check each face to see if it is the one that connected
            for (int i = 0; i < 6; i++)
            {
                if (distances[i] < penetration)
                {
                    // determine the penetration distance
                    penetration = distances[i];
                    face = faces[i];
                }
            }
            
            // set the contact properties
            contactB.face = face;
            contactB.penetration = penetration;


            // check if contact does not exist
            if (!a.contacts.Contains(contactB))
            {
                // remove any contact that matches the name but not other parameters
                for (int i = a.contacts.Count - 1; i > -1; i--)
                {
                    if (a.contacts[i].cube.name.Equals(contactB.cube.name))
                    {
                        a.contacts.RemoveAt(i);
                    }
                }

                if (contactB.face == Vector3.down)
                {
                    a.gameObject.GetComponent<RigidBody3D>().Stop();
                    a.isGrounded = true;
                }
                else //------------------------------PUSH -------------------------------
                {
                    if(contactB.face == Vector3.forward)
                    {
                        b.transform.position = new Vector3(b.transform.position.x,b.transform.position.y,b. transform.position.z + contactB.penetration);
                        Debug.Log("Collision Forward");
                    }
                    else if(contactB.face == Vector3.back)
                    {
                        b.transform.position = new Vector3(b.transform.position.x,b.transform.position.y,b. transform.position.z - contactB.penetration);
                        Debug.Log("Collision back");
                    }
                    else if(contactB.face == Vector3.right)
                    {
                        b.transform.position = new Vector3(b.transform.position.x + contactB.penetration,b.transform.position.y,b. transform.position.z);
                        Debug.Log("Collision Right");
                    }
                    else if(contactB.face == Vector3.left)
                    {
                        b.transform.position = new Vector3(b.transform.position.x - contactB.penetration,b.transform.position.y,b. transform.position.z );
                        Debug.Log("Collision Left");
                    }
                }
                
                

                // add the new contact
                a.contacts.Add(contactB);
                a.isColliding = true;
                
            }
        }
        else
        {

            if (a.contacts.Exists(x => x.cube.gameObject.name == b.gameObject.name))
            {
                a.contacts.Remove(a.contacts.Find(x => x.cube.gameObject.name.Equals(b.gameObject.name)));
                a.isColliding = false;

                if (a.gameObject.GetComponent<RigidBody3D>().bodyType == BodyType.DYNAMIC)
                {
                    a.gameObject.GetComponent<RigidBody3D>().isFalling = true;
                    a.isGrounded = false;
                }
            }
        }
    }
}
