using UnityEngine;
using System.Collections;

public class WeaponSystem : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public string name;
        public GameObject weaponModel;
        public float damage;
        public float fireRate;
        public float range;
        public int maxAmmo;
        public int currentAmmo;
        public float reloadTime;
        public AudioClip shootSound;
        public AudioClip reloadSound;
        public ParticleSystem muzzleFlash;
    }
    
    [Header("Weapon Settings")]
    public Weapon[] availableWeapons;
    public int currentWeaponIndex = 0;
    public Transform weaponHolder;
    
    [Header("Effects")]
    public AudioSource audioSource;
    public Camera playerCamera;
    
    private bool isReloading = false;
    private float nextFireTime = 0f;
    
    private void Start()
    {
        if (availableWeapons.Length > 0)
        {
            EquipWeapon(0);
        }
    }
    
    private void Update()
    {
        if (isReloading)
            return;
            
        // Handle weapon switching
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0)
        {
            int newIndex = currentWeaponIndex + (scrollWheel > 0 ? 1 : -1);
            if (newIndex < 0) newIndex = availableWeapons.Length - 1;
            if (newIndex >= availableWeapons.Length) newIndex = 0;
            EquipWeapon(newIndex);
        }
        
        // Handle shooting
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
        }
        
        // Handle reloading
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
        }
    }
    
    private void EquipWeapon(int index)
    {
        // Destroy current weapon model
        foreach (Transform child in weaponHolder)
        {
            Destroy(child.gameObject);
        }
        
        currentWeaponIndex = index;
        Weapon currentWeapon = availableWeapons[currentWeaponIndex];
        
        // Instantiate new weapon model
        if (currentWeapon.weaponModel != null)
        {
            Instantiate(currentWeapon.weaponModel, weaponHolder);
        }
    }
    
    private void Shoot()
    {
        Weapon currentWeapon = availableWeapons[currentWeaponIndex];
        
        if (currentWeapon.currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        
        nextFireTime = Time.time + 1f / currentWeapon.fireRate;
        currentWeapon.currentAmmo--;
        
        // Play effects
        if (currentWeapon.muzzleFlash != null)
            currentWeapon.muzzleFlash.Play();
            
        if (currentWeapon.shootSound != null && audioSource != null)
            audioSource.PlayOneShot(currentWeapon.shootSound);
            
        // Raycast for hit detection
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, currentWeapon.range))
        {
            // Check if we hit another player
            OtterPlayerController hitPlayer = hit.transform.GetComponent<OtterPlayerController>();
            if (hitPlayer != null)
            {
                // Apply damage to player
                // TODO: Implement player health system
            }
        }
    }
    
    private IEnumerator Reload()
    {
        Weapon currentWeapon = availableWeapons[currentWeaponIndex];
        
        isReloading = true;
        
        if (currentWeapon.reloadSound != null && audioSource != null)
            audioSource.PlayOneShot(currentWeapon.reloadSound);
            
        yield return new WaitForSeconds(currentWeapon.reloadTime);
        
        currentWeapon.currentAmmo = currentWeapon.maxAmmo;
        isReloading = false;
    }
} 