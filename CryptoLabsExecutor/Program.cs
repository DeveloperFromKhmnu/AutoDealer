using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CryptoLabsExecutor;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("================================================================================");
        Console.WriteLine(" ДОСЛІДНИЦЬКИЙ КОМПЛЕКС КІБЕРБЕЗПЕКИ: ЛР №4, №6, №7 ДЛЯ СТУДЕНТА ЧЕРНЯЄВА");
        Console.WriteLine(" Група: КБЗІ-23-2, Прізвище: Cherniajev, Варіант: 01 (умовний)");
        Console.WriteLine("================================================================================\n");

        RunLab4();
        Console.WriteLine("\n--------------------------------------------------------------------------------\n");
        RunLab6();
        Console.WriteLine("\n--------------------------------------------------------------------------------\n");
        RunLab7();
        Console.WriteLine("\n--------------------------------------------------------------------------------\n");
        GenerateLicenseFiles();

        Console.WriteLine("\nВсі завдання успішно виконані!");
    }

    #region ЛАБОРАТОРНА РОБОТА №4: ХЕШУВАННЯ ТА АВТЕНТИФІКАЦІЯ (BCrypt vs SHA-256)

    private static void RunLab4()
    {
        Console.WriteLine(">>> [ЛР №4] ЗАВДАННЯ 1-13: ДОСЛІДЖЕННЯ BCRYPT ТА АТАКА ПЕРЕБОРОМ <<<");

        // 2. Сформувати текстовий рядок: [рік]_[група]_[прізвище]_[варіант]_
        var year = "2026";
        var group = "КБЗІ-23-2";
        var surname = "Cherniajev";
        var variant = "01";
        var initialStr = $"{year}_{group}_{surname}_{variant}_";

        var initialBytes = Encoding.UTF8.GetBytes(initialStr);
        Console.WriteLine($"1. Базовий рядок: \"{initialStr}\"");
        Console.WriteLine($"   Початковий розмір в UTF-8: {initialBytes.Length} байтів.");

        // 3. Доповнити символами '_' до 72 байтів
        var padCount = 72 - initialBytes.Length;
        if (padCount < 0) padCount = 0;
        var s1 = initialStr + new string('_', padCount);
        var s1Bytes = Encoding.UTF8.GetBytes(s1);
        Console.WriteLine($"2. Рядок №1 (доповнений до 72 байтів): \"{s1}\"");
        Console.WriteLine($"   Розмір S1: {s1Bytes.Length} байтів.");

        // 5. Доповнити базовий рядок символом '1' (і до 72 байт)
        var s2 = s1 + "1"; // 73 байти
        Console.WriteLine($"3. Рядок №2 (S1 + '1', 73 байти): \"{s2}\"");

        // 6. Доповнити S1 10 псевдовипадковими символами
        var s3 = s1 + "X9aB7cQ2wZ"; // 82 байти
        Console.WriteLine($"4. Рядок №3 (S1 + 10 випадкових символів, 82 байти): \"{s3}\"");

        // 7. Видалити з кінця S1 один символ '_'
        var s4 = s1[..^1]; // 71 байт
        Console.WriteLine($"5. Рядок №4 (S1 без останнього '_', 71 байт): \"{s4}\"");

        Console.WriteLine("\nГенерація BCrypt-хешів (cost = 10)...");
        var h1 = BCrypt.Net.BCrypt.HashPassword(s1, workFactor: 10);
        var h2 = BCrypt.Net.BCrypt.HashPassword(s2, workFactor: 10);
        var h3 = BCrypt.Net.BCrypt.HashPassword(s3, workFactor: 10);
        var h4 = BCrypt.Net.BCrypt.HashPassword(s4, workFactor: 10);

        Console.WriteLine($"   Хеш №1 (S1): {h1}");
        Console.WriteLine($"   Хеш №2 (S2): {h2}");
        Console.WriteLine($"   Хеш №3 (S3): {h3}");
        Console.WriteLine($"   Хеш №4 (S4): {h4}");

        // 8. Попарна перевірка 4 хешів відносно 4 рядків
        Console.WriteLine("\n--- Матриця попарної перевірки (BCrypt.Verify) ---");
        var strings = new[] { ("S1 (72b)", s1), ("S2 (73b)", s2), ("S3 (82b)", s3), ("S4 (71b)", s4) };
        var hashes = new[] { ("H1 (з S1)", h1), ("H2 (з S2)", h2), ("H3 (з S3)", h3), ("H4 (з S4)", h4) };

        Console.Write("{0,-12}", "Рядок \\ Хеш");
        foreach (var (hName, _) in hashes) Console.Write("{0,-15}", hName);
        Console.WriteLine();

        foreach (var (sName, sVal) in strings)
        {
            Console.Write("{0,-12}", sName);
            foreach (var (_, hVal) in hashes)
            {
                var match = BCrypt.Net.BCrypt.Verify(sVal, hVal);
                Console.Write("{0,-15}", match ? "TRUE (Збіг)" : "false");
            }
            Console.WriteLine();
        }

        Console.WriteLine("\nВИСНОВОК ЩОДО BCRYPT (п. 8):");
        Console.WriteLine(" - Алгоритм BCrypt має внутрішнє архітектурне обмеження: він обробляє лише ПЕРШІ 72 БАЙТИ пароля.");
        Console.WriteLine(" - Оскільки S1, S2 та S3 ідентичні в перших 72 байтах, їхні хеші H1, H2, H3 є взаємно сумісними!");
        Console.WriteLine(" - Рядок S4 відрізняється на 71-му байті, тому жоден хеш H1-H3 не підходить до S4, і навпаки.");

        // 9-13. Атака перебором: pass: Cherniajev
        var targetPass = "pass: Cherniajev";
        Console.WriteLine($"\n--- Атака перебором за словником (п. 9-13) для \"{targetPass}\" ---");

        var sha256Target = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(targetPass))).ToLowerInvariant();
        Console.WriteLine($"Хеш №5 (SHA-256): {sha256Target}");

        Console.WriteLine("Обчислення Хеш №6 (BCrypt cost 12, зачекайте)...");
        var bcryptTarget = BCrypt.Net.BCrypt.HashPassword(targetPass, workFactor: 12);
        Console.WriteLine($"Хеш №6 (BCrypt 12): {bcryptTarget}");

        // Словник із 20 кандидатів (20-й — правильний)
        var dictionary = new List<string>
        {
            "pass: admin", "pass: 123456", "pass: password", "pass: qwerty", "pass: secret",
            "pass: test", "pass: user", "pass: student", "pass: security", "pass: crm2026",
            "pass: copart", "pass: iaai", "pass: dealer", "pass: kyiv", "pass: ukraine",
            "pass: kbzi23", "pass: cherniaev", "pass: chernyaev", "pass: master", "pass: Cherniajev"
        };

        // Атака на SHA-256
        var sw = Stopwatch.StartNew();
        string foundSha = "";
        int attemptsSha = 0;
        foreach (var cand in dictionary)
        {
            attemptsSha++;
            var candHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(cand))).ToLowerInvariant();
            if (candHash == sha256Target)
            {
                foundSha = cand;
                break;
            }
        }
        sw.Stop();
        var shaTime = sw.Elapsed;
        Console.WriteLine($"\n[SHA-256] Знайдено: \"{foundSha}\" за {attemptsSha} спроб. Час: {shaTime.TotalMilliseconds:F4} мс ({shaTime.Ticks} тактів)");

        // Атака на BCrypt (cost 12)
        sw.Restart();
        string foundBcrypt = "";
        int attemptsBcrypt = 0;
        foreach (var cand in dictionary)
        {
            attemptsBcrypt++;
            if (BCrypt.Net.BCrypt.Verify(cand, bcryptTarget))
            {
                foundBcrypt = cand;
                break;
            }
        }
        sw.Stop();
        var bcryptTime = sw.Elapsed;
        Console.WriteLine($"[BCrypt 12] Знайдено: \"{foundBcrypt}\" за {attemptsBcrypt} спроб. Час: {bcryptTime.TotalMilliseconds:F2} мс ({bcryptTime.TotalSeconds:F2} сек)");

        Console.WriteLine("\nВИСНОВОК ЩОДО СТІЙКОСТІ ДО ПЕРЕБОРУ (п. 13):");
        Console.WriteLine($" - SHA-256 є надзвичайно швидкою хеш-функцією (~{shaTime.TotalMilliseconds:F3} мс на 20 спроб), тому легко піддається атаці грубою силою.");
        Console.WriteLine($" - BCrypt навмисно ресурсоємний (~{bcryptTime.TotalMilliseconds / 20:F1} мс на 1 перевірку при cost 12), що робить масовий перебір у мільйони разів дорожчим і неможливим на практиці.");
    }

    #endregion

    #region ЛАБОРАТОРНА РОБОТА №6: СИМЕТРИЧНИЙ ЗАХИСТ (AES-CTR ТА АТАКА ВІДОМОГО ВІДКРИТОГО ТЕКСТУ)

    private static void RunLab6()
    {
        Console.WriteLine(">>> [ЛР №6] СИМЕТРИЧНИЙ КРИПТОГРАФІЧНИЙ ЗАХИСТ ТА АТАКА НА AES-CTR <<<");

        var text1 = "2026_КБЗІ-23-2_Cherniajev_Dmytro_варіант:01";
        Console.WriteLine($"1. Оригінальний відкритий текст: \"{text1}\"");

        var key = new byte[32]; // AES-256
        var nonce = new byte[16]; // 128-bit counter block
        RandomNumberGenerator.Fill(key);
        RandomNumberGenerator.Fill(nonce);

        Console.WriteLine($"   Ключ №1 (HEX): {Convert.ToHexString(key)}");
        Console.WriteLine($"   Вектор №1 (HEX): {Convert.ToHexString(nonce)}");

        // Реалізація AES-CTR за стандартом NIST SP 800-38A
        var c1 = AesCtrTransform(Encoding.UTF8.GetBytes(text1), key, nonce);
        Console.WriteLine($"2. Шифротекст №1 (HEX): {Convert.ToHexString(c1)}");

        // Атака відомого відкритого тексту (Known Plaintext Attack):
        // Gamma = Ciphertext XOR Plaintext
        var p1Bytes = Encoding.UTF8.GetBytes(text1);
        var gamma = new byte[c1.Length];
        for (int i = 0; i < c1.Length; i++) gamma[i] = (byte)(c1[i] ^ p1Bytes[i]);
        Console.WriteLine($"3. Отримана Гама №1 (Keystream) через XOR C1 ^ P1 (HEX): {Convert.ToHexString(gamma)}");

        // Альтернативний відкритий текст: замінити варіант:01 на варіант:99
        var text2 = "2026_КБЗІ-23-2_Cherniajev_Dmytro_варіант:99";
        var p2Bytes = Encoding.UTF8.GetBytes(text2);

        // Підроблений шифротекст C2 = Gamma XOR P2
        var c2 = new byte[p2Bytes.Length];
        for (int i = 0; i < p2Bytes.Length; i++) c2[i] = (byte)(gamma[i] ^ p2Bytes[i]);
        Console.WriteLine($"4. Сформований зловмисником Шифротекст №2 (HEX): {Convert.ToHexString(c2)}");

        // Перевірка: жертва розшифровує C2 оригінальним ключем та вектором
        var decryptedBytes = AesCtrTransform(c2, key, nonce);
        var decryptedText = Encoding.UTF8.GetString(decryptedBytes);
        Console.WriteLine($"5. Результат розшифрування Шифротексту №2 ключем жертви: \"{decryptedText}\"");

        Console.WriteLine("\nВИСНОВОК ЩОДО РЕЖИМУ AES-CTR (п. 6):");
        Console.WriteLine(" - Режим CTR є потоковим режимом шифрування, який забезпечує ЛИШЕ конфіденційність, але НЕ забезпечує цілісність (malleability).");
        Console.WriteLine(" - Знаючи пару (P1, C1), зловмисник може відновити гаму і підмінити довільний шифротекст без знання ключа.");
        Console.WriteLine(" - САМЕ ТОМУ в нашому веб-проєкті AutoDealerCRM застосовано автентифіковане шифрування AES-256-GCM, яке містить автентифікаційний тег і унеможливлює таку атаку!");
    }

    private static byte[] AesCtrTransform(byte[] input, byte[] key, byte[] nonce)
    {
        var output = new byte[input.Length];
        using var aes = Aes.Create();
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.None;

        var counter = (byte[])nonce.Clone();
        using var encryptor = aes.CreateEncryptor(key, null);

        var block = new byte[16];
        var encryptedCounter = new byte[16];

        for (int offset = 0; offset < input.Length; offset += 16)
        {
            encryptor.TransformBlock(counter, 0, 16, encryptedCounter, 0);

            var count = Math.Min(16, input.Length - offset);
            for (int i = 0; i < count; i++)
                output[offset + i] = (byte)(input[offset + i] ^ encryptedCounter[i]);

            // Increment 128-bit big-endian counter
            for (int i = 15; i >= 0; i--)
            {
                if (++counter[i] != 0) break;
            }
        }
        return output;
    }

    #endregion

    #region ЛАБОРАТОРНА РОБОТА №7: КОНТРОЛЬ ЦІЛІСНОСТІ ТА АВТЕНТИЧНОСТІ (SHA-256, HMAC, AES-GCM)

    private static void RunLab7()
    {
        Console.WriteLine(">>> [ЛР №7] МЕТОДИ КОНТРОЛЮ ЦІЛІСНОСТІ ТА АВТЕНТИЧНОСТІ <<<");

        var text = "2026_КБЗІ-23-2_Cherniajev_Dmytro_варіант:01";
        Console.WriteLine($"1. Оригінальний рядок: \"{text}\"");

        // 3. Хеш №1 (SHA-256)
        var hash1 = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        Console.WriteLine($"   Хеш №1 (SHA-256): {Convert.ToHexString(hash1)}");

        // 4. Зміна 1 символу ('01' -> '02')
        var modifiedText = "2026_КБЗІ-23-2_Cherniajev_Dmytro_варіант:02";
        var hash2 = SHA256.HashData(Encoding.UTF8.GetBytes(modifiedText));
        Console.WriteLine($"   Хеш №2 (SHA-256, змінено 1 символ): {Convert.ToHexString(hash2)}");

        var bitDiff = CountBitDifferences(hash1, hash2);
        Console.WriteLine($"   Лавинний ефект (Avalanche effect): змінено {bitDiff} бітів із 256 ({bitDiff / 256.0 * 100:F1}%)!");

        // 5. HMAC-SHA256 (Ключ №1)
        var hmacKey1 = new byte[32];
        RandomNumberGenerator.Fill(hmacKey1);
        var hmac1 = HMACSHA256.HashData(hmacKey1, Encoding.UTF8.GetBytes(text));
        Console.WriteLine($"\n2. HMAC №1 (Ключ №1): {Convert.ToHexString(hmac1)}");

        // 6. Зміна 1 символу повідомлення та перевірка відносно HMAC №1
        var hmacModifiedMsg = HMACSHA256.HashData(hmacKey1, Encoding.UTF8.GetBytes(modifiedText));
        var matchMsg = CryptographicOperations.FixedTimeEquals(hmac1, hmacModifiedMsg);
        Console.WriteLine($"   Перевірка HMAC при зміні 1 символу вхідного тексту: {(matchMsg ? "ЗБІГ" : "НЕ ЗБІГАЄТЬСЯ (Порушення цілісності виявлено!)")}");

        // 7. Зміна 1 біта ключа
        var hmacKey2 = (byte[])hmacKey1.Clone();
        hmacKey2[0] ^= 0x01; // інверсія 1 біта
        var hmac2 = HMACSHA256.HashData(hmacKey2, Encoding.UTF8.GetBytes(text));
        Console.WriteLine($"3. HMAC №2 (Ключ зі зміненим 1 бітом): {Convert.ToHexString(hmac2)}");

        var bitDiffKey = CountBitDifferences(hmac1, hmac2);
        Console.WriteLine($"   Лавинний ефект у ключі HMAC: зміна лише 1 біта ключа змінила {bitDiffKey} бітів результату ({bitDiffKey / 256.0 * 100:F1}%)!");

        // 10-11. Автентифіковане шифрування AES-GCM
        Console.WriteLine("\n4. Автентифіковане шифрування AES-256-GCM:");
        var gcmKey = new byte[32];
        var gcmNonce = new byte[12];
        RandomNumberGenerator.Fill(gcmKey);
        RandomNumberGenerator.Fill(gcmNonce);

        var ptBytes = Encoding.UTF8.GetBytes(text);
        var ctBytes = new byte[ptBytes.Length];
        var tag = new byte[16];

        using (var aesGcm = new AesGcm(gcmKey, 16))
        {
            aesGcm.Encrypt(gcmNonce, ptBytes, ctBytes, tag);
        }

        Console.WriteLine($"   Шифротекст (HEX): {Convert.ToHexString(ctBytes)}");
        Console.WriteLine($"   Автентифікаційна мітка (Tag, 128 bit): {Convert.ToHexString(tag)}");

        // 11. Зміна одного байта в шифротексті
        var tamperedCt = (byte[])ctBytes.Clone();
        tamperedCt[0] ^= 0xFF; // псуємо перший байт

        Console.WriteLine("   Спроба розшифрування пошкодженого шифротексту...");
        try
        {
            var recovered = new byte[tamperedCt.Length];
            using var aesGcm = new AesGcm(gcmKey, 16);
            aesGcm.Decrypt(gcmNonce, tamperedCt, tag, recovered);
            Console.WriteLine("   УСПІХ (Помилка! Модифікація не була помічена)");
        }
        catch (CryptographicException)
        {
            Console.WriteLine("   ✓ СПІЙМАНО: CryptographicException! Автентифікаційна мітка відкинула модифікований шифротекст.");
            Console.WriteLine("   Цілісність та автентичність успішно захищені механізмом AES-GCM.");
        }
    }

    private static int CountBitDifferences(byte[] a, byte[] b)
    {
        int diff = 0;
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            byte xor = (byte)(a[i] ^ b[i]);
            while (xor != 0)
            {
                diff += xor & 1;
                xor >>= 1;
            }
        }
        return diff;
    }

    #endregion

    #region ГЕНЕРАЦІЯ ФАЙЛІВ ЛІЦЕНЗІЙ ДЛЯ ВЕБ-ДОДАТКА (dealer.lic)

    private static void GenerateLicenseFiles()
    {
        Console.WriteLine(">>> СТВОРЕННЯ ТЕСТОВИХ ЛІЦЕНЗІЙНИХ ФАЙЛІВ ДЛЯ AUTODEALER CRM <<<");
        var keyBase64 = "yZ2TAVqB8U87+QznVdUgQhkwKX/KJhMletPb/B/IePQ=";
        var key = Convert.FromBase64String(keyBase64);

        var licensesDir = @"c:\Users\Asus\Desktop\БПЗ_Черняєв\AutoDealerCRM\Licenses";
        Directory.CreateDirectory(licensesDir);

        // 1. Валідна ліцензія
        var owner = "Cherniajev Auto Import LLC (КБЗІ-23-2)";
        var expiresAt = "2028-12-31T23:59:59Z";
        var msg = $"{owner}|{expiresAt}";
        var sigBytes = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(msg));
        var sigBase64 = Convert.ToBase64String(sigBytes);

        var validLicense = new
        {
            Owner = owner,
            ExpiresAt = expiresAt,
            Signature = sigBase64
        };

        var validPath = Path.Combine(licensesDir, "dealer_valid.lic");
        File.WriteAllText(validPath, JsonSerializer.Serialize(validLicense, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"1. Створено валідну ліцензію: {validPath}");

        // 2. Підроблена ліцензія (модифікований власник без зміни підпису)
        var tamperedLicense = new
        {
            Owner = "Зловмисник Хакер Іванович",
            ExpiresAt = expiresAt,
            Signature = sigBase64 // чужий підпис!
        };

        var tamperedPath = Path.Combine(licensesDir, "dealer_tampered.lic");
        File.WriteAllText(tamperedPath, JsonSerializer.Serialize(tamperedLicense, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"2. Створено підроблену ліцензію (для демонстрації відхилення): {tamperedPath}");

        // 3. Прострочена ліцензія
        var expiredDate = "2023-01-01T00:00:00Z";
        var expiredMsg = $"{owner}|{expiredDate}";
        var expiredSig = Convert.ToBase64String(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(expiredMsg)));
        var expiredLicense = new
        {
            Owner = owner,
            ExpiresAt = expiredDate,
            Signature = expiredSig
        };

        var expiredPath = Path.Combine(licensesDir, "dealer_expired.lic");
        File.WriteAllText(expiredPath, JsonSerializer.Serialize(expiredLicense, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"3. Створено прострочену ліцензію: {expiredPath}");
    }

    #endregion
}
