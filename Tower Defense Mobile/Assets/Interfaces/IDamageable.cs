﻿using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable<T> {

    void TakeDamage(T damageTaken);

    void Die();

}
