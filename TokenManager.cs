using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TokenManager
{
    private static string token;

    public static void SetToken(string newToken)
    {
        token = newToken;
    }

    public static string GetToken()
    {
        return token;
    }
}
