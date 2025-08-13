using System;               
using UnityEngine;           
//todo: remove explenation comments before final commit

// A simple helper that gives you a stable, per-install token.
// You do NOT add this as a component; it's a plain static utility.
public static class SessionTokenUtil
{
    // The key name we use to store the token in PlayerPrefs
    private const string Key = "Fusion_ClientToken";

    // Get the token as a byte array (what Fusion expects for ConnectionToken)
    public static byte[] GetConnectionTokenBytes()
    {
        long token = GetOrCreateLongToken();          // Make sure we have a token
        return BitConverter.GetBytes(token);          // Convert long -> byte[]
    }

    // Get the saved token, or create and save a new one if none exists
    public static long GetOrCreateLongToken()
    {
        // If no token saved yet, create one and store it
        if (!PlayerPrefs.HasKey(Key))
        {
            long value = GenerateRandomLong();        // Make a new random-ish 64-bit number
            PlayerPrefs.SetString(Key, value.ToString()); // Save as string (PlayerPrefs has no "long")
            PlayerPrefs.Save();                       // Flush to disk
            return value;                             // Use it
        }

        // We have something saved—try to read and parse it
        string stored = PlayerPrefs.GetString(Key, "0");
        
        if (long.TryParse(stored, out long parsed))   // If it's a valid long
            return parsed;                            // Use the saved token

        // If the saved value was somehow bad, create a fresh one and save it again
        long fallback = GenerateRandomLong();
        PlayerPrefs.SetString(Key, fallback.ToString());
        PlayerPrefs.Save();
        return fallback;
    }

    // Make a simple 64-bit number by combining two random 32-bit ints
    private static long GenerateRandomLong()
    {
        int a = UnityEngine.Random.Range(int.MinValue, int.MaxValue); 
        int b = UnityEngine.Random.Range(int.MinValue, int.MaxValue); 

        // Move 'a' up into the high 32 bits, and merge 'b' into the low 32 bits.
        // Casting 'b' to uint avoids negative-number sign issues.
        return ((long)a << 32) ^ (uint)b; // Result is a 64-bit long with a on top and b on bottom
    }
}
