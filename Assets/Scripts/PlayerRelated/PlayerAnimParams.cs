using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CONTRATO de parâmetros do Animator, igual para TODOS os personagens.
///
/// Regras:
///  - Todo Animator Controller de personagem usa estes nomes EXATOS.
///  - Parâmetros extras de um personagem (ex: "HammerSlam") ficam no script
///    filho dele, nunca aqui.
///  - Nunca escreva o nome do parâmetro como string em outro lugar: use sempre
///    PlayerAnimParams.Algo.
/// </summary>
public static class PlayerAnimParams
{
    // Lista dos parâmetros obrigatórios (usada para avisar quando falta algum no Controller).
    // Precisa vir ANTES dos campos abaixo por causa da ordem de inicialização.
    static readonly List<string> required = new List<string>();
    public static IReadOnlyList<string> Required => required;

    static int Hash(string name, bool isRequired = true)
    {
        if (isRequired) required.Add(name);
        return Animator.StringToHash(name);
    }

    // -------------------------------------------------------------------------
    // LOCOMOÇÃO (preenchidos automaticamente pelo PlayerAnimation)
    // -------------------------------------------------------------------------
    public static readonly int Speed         = Hash("Speed");          // float  |vx|
    public static readonly int VerticalSpeed = Hash("VerticalSpeed");  // float  vy (sobe > 0, cai < 0)
    public static readonly int PlaybackSpeed = Hash("PlaybackSpeed");  // float  multiplicador de Walk/Run (default = 1!)
    public static readonly int Grounded      = Hash("Grounded");       // bool
    public static readonly int Running       = Hash("Running");        // bool
    public static readonly int WallSliding   = Hash("WallSliding");    // bool
    public static readonly int Jump          = Hash("Jump");           // trigger (dispara em todo pulo, inclusive duplo e wall jump)

    // -------------------------------------------------------------------------
    // CONVENÇÕES OPCIONAIS (nomes reservados; disparados pelo script do personagem)
    // -------------------------------------------------------------------------
    public static readonly int Attack = Hash("Attack", false);  // trigger
    public static readonly int Hurt   = Hash("Hurt", false);    // trigger
    public static readonly int Dead   = Hash("Dead", false);    // bool
}
