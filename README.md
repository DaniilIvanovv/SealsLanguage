SealsLanguage — ASCII Instruction
This document uses ASCII-only text to ensure correct rendering in any PDF viewer. All examples below
can be copied directly into the SealsLanguage REPL.

1. Numbers and arithmetic

2 + 3 * 4
(2 + 3) * 4
10 / 4
2 ^ 3
sqrt(16)

2. Variables

let a = 10
let b = 5
a + b * 2

3. Logic and comparison

5 > 3
10 == 10
10 != 5
true and false
not (5 < 3)

4. Conditional expressions

if(5 > 3, "greater", "less")
if(false, 1, 0)
let x = 7; if(x > 5, "big", "small")

5. Strings

"hello" + " world"
upper("seal")
lower("LANGUAGE")
len("SealsLanguage")
substr("SealsLanguage", 5, 8)

6. Math functions

sin(0)
cos(0)
pow(2, 8)
max(10, 20)
min(3, 7)
abs(-42)
round(3.6)

7. Combined examples

let r = 10
3.14159 * r ^ 2
let name = "seal"
upper(concat(name, "s"))
if(len("test") == 4 and 2 < 3, "ok", "fail")

End of ASCII-safe documentation.


⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⠿⠿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠋⠁⠄⠄⠄⠄⠄⠹⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠄⠄⠄⠄⣠⡶⠄⣀⡄⢻⣿⣿⣿
⣿⣿⣿⣿⣿⡿⠿⢿⣿⡟⠄⠄⢀⠄⠄⣽⣷⣶⡟⠁⢸⣿⣿⣿
⣿⣿⢿⣿⡿⠁⠄⠄⣿⣧⠄⠐⢢⡷⠄⠿⣿⡿⠇⠄⢸⣿⣿⣿
⡟⠄⠄⠙⠃⠄⠄⢠⣿⣿⡀⠄⠈⠓⢤⣤⣾⣠⣪⢶⢸⣿⣿⣿
⣧⠄⠄⠄⠄⠄⣠⣿⣿⣿⠃⠄⠄⠄⢸⠛⢿⠏⠁⠄⠘⣿⣿⣿
⣿⣷⣦⡄⠄⠄⠈⠛⠛⠋⠄⠄⠄⠄⠄⠉⠁⠄⠄⠄⠄⢹⣿⣿
⣿⣿⣿⣇⠄⠄⠄⠄⠄⢀⡀⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢀⣿⣿
⣿⣿⣿⣿⡀⠄⠄⠄⢠⠏⠄⠄⠄⢀⠄⠄⠄⠄⠄⠄⢀⡜⠹⣿
⣿⣿⣿⣿⣷⣄⠄⣠⠼⠄⠄⠄⠄⢸⠄⠄⠄⠄⠄⣠⠞⠄⠄⢸
⣿⣿⣿⣿⣿⣿⣿⡁⠄⠄⠄⢀⣠⣋⣀⣠⣤⣴⣾⣿⣶⣤⣴⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿


