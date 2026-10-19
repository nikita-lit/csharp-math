(() => {
    // Estonian is the source language for views and mathematical explanations.
    const ru = {
        'Põhinavigatsioon': 'Основная навигация',
        'Ava navigatsioon': 'Открыть меню',
        'Kalkulaator': 'Калькулятор',
        'Salvestatud': 'Сохранённое',
        'Teooria': 'Теория',
        'Keel': 'Язык',
        'Tume teema': 'Тёмная тема',
        'Funktsiooni kalkulaator': 'Калькулятор функций',
        'Sisesta valem ja vaata tulemusi koos lahenduskäiguga allpool.': 'Введите формулу — результаты и решение появятся ниже.',
        'Sisesta funktsioon': 'Введите функцию',
        'Kirjuta valem või vali näide.': 'Напишите формулу или выберите пример.',
        'Funktsiooni valem': 'Формула функции',
        'Kursor vasakule': 'Курсор влево',
        'Kursor paremale': 'Курсор вправо',
        'Näidisfunktsioon': 'Пример функции',
        'Vali näide…': 'Выберите пример…',
        'Polünoomid': 'Многочлены',
        'Ratsionaalfunktsioonid': 'Рациональные функции',
        'Eksponentfunktsioonid': 'Показательные функции',
        'Juurfunktsioonid': 'Функции с корнями',
        'Logaritm ja trigonomeetria': 'Логарифм и тригонометрия',
        'Kasuta muutujat x, sulge ja tehteid + − * / ^. Näiteks 2x või sqrt(x).': 'Используйте переменную x, скобки и операции + − * / ^. Например: 2x или sqrt(x).',
        'Põhifunktsioonid': 'Основные функции',
        'Arvud ja tehted': 'Числа и операции',
        'Puhasta valem': 'Очистить формулу',
        'Kustuta eelmine märk': 'Удалить предыдущий символ',
        'Vahemiku seaded (valikuline)': 'Настройки диапазона (необязательно)',
        'Vahemiku seaded': 'Настройки диапазона',
        'Uurimine lõigul': 'Исследование на отрезке',
        'Ainult valitud lõik': 'Только выбранный отрезок',
        'Funktsiooni uuritakse sellel kinnisel lõigul, sealhulgas otspunktides.': 'Функция исследуется на этом замкнутом отрезке, включая его концы.',
        'Funktsiooni uuritakse kogu määramispiirkonnas. x-vahemik määrab graafiku akna.': 'Функция исследуется на всей области определения. Диапазон x задаёт окно графика.',
        'x alates': 'x от',
        'x kuni': 'x до',
        'Arvuta': 'Рассчитать',
        'Arvutan…': 'Вычисление…',
        'Salvesta uurimine': 'Сохранить исследование',
        'Salvesta muudatused': 'Сохранить изменения',
        'Tulemused ja lahenduskäik': 'Результаты и решение',
        'Iga omaduse vastus koos arvutuste ja selgitustega.': 'Ответ по каждому свойству с вычислениями и пояснениями.',
        'Funktsiooni graafik': 'График функции',
        'Sisesta funktsiooni valem.': 'Введите формулу функции.',
        'Graafikul on märgitud nullkohad, ekstreemumid ja käänupunktid.': 'На графике отмечены нули, экстремумы и точки перегиба.',
        'Lahenduskäik ilmub pärast funktsiooni arvutamist.': 'Решение появится после расчёта функции.',
        'Sisesta valem, et näha funktsiooni omadusi.': 'Введите формулу, чтобы увидеть свойства функции.',
        'Sisesta valem ja uuri selle omadusi.': 'Введите формулу и исследуйте её свойства.',
        'Tulemused kehtivad näidatud määramispiirkonnas.': 'Результаты относятся к указанной области определения.',
        'Osa tulemusi on leitud numbriliselt valitud vahemikus.': 'Часть результатов найдена численно в выбранном диапазоне.',
        'Osa tulemusi on leitud numbriliselt vahemikus': 'Часть результатов найдена численно в диапазоне',
        'Arvutus valmis.': 'Расчёт завершён.',
        'Uurin funktsiooni…': 'Исследование функции…',
        'Koostan lahenduskäiku…': 'Подготовка решения…',
        'Sisend muutus. Vajuta Arvuta, et tulemusi uuendada.': 'Данные изменились. Нажмите «Рассчитать», чтобы обновить результаты.',
        'Arvutamine ei õnnestunud. Kontrolli sisendit.': 'Расчёт не удался. Проверьте введённые данные.',
        'Vastus': 'Ответ',
        'Lahenduskäik': 'Ход решения',
        'Kontroll': 'Проверка',
        'Numbriliselt': 'Численно',
        'Salvestatud funktsioonid': 'Сохранённые функции',
        'Uus uurimine': 'Новое исследование',
        'Ühtegi funktsiooni pole veel salvestatud.': 'Пока нет сохранённых функций.',
        'Ava kalkulaator': 'Открыть калькулятор',
        'ja alusta uurimist.': 'и начать исследование.',
        'Funktsioon': 'Функция',
        'Tuletis': 'Производная',
        'Tegevused': 'Действия',
        'Ava': 'Открыть',
        'Muuda funktsiooni': 'Изменить функцию',
        'Muuda valemit või uurimispiirkonda ja vaata uuendatud lahendust.': 'Измените формулу или диапазон и посмотрите обновлённое решение.',
        'Muuda': 'Изменить',
        'Kustuta': 'Удалить',
        'Funktsiooni uurimine': 'Исследование функции',
        'Uurimine': 'Исследование',
        'Muudetud': 'Изменено',
        'Tagasi uurimise juurde': 'Вернуться к исследованию',
        'Kustuta uurimine': 'Удалить исследование',
        'Kustuta see uurimine?': 'Удалить это исследование?',
        'Salvestatud valem ja selle tulemused eemaldatakse sinu kogust.': 'Сохранённая формула и её результаты будут удалены из вашей коллекции.',
        'Jäta alles': 'Оставить',
        'Arvutamine katkes': 'Расчёт прерван',
        'MIDAGI LÄKS VALESTI': 'ПРОИЗОШЛА ОШИБКА',
        'Proovime uuesti.': 'Попробуем ещё раз.',
        'Päringu töötlemine ei õnnestunud. Ava lahendaja ja proovi uuesti.': 'Не удалось обработать запрос. Откройте калькулятор и попробуйте снова.',
        'Tagasi lahendajasse': 'Вернуться к калькулятору',
        'Päringu tunnus': 'Идентификатор запроса',
        'Määramispiirkond ja muutumispiirkond': 'Область определения и область значений',
        'Määramis- ja muutumispiirkond': 'Область определения и область значений',
        'Määramispiirkond': 'Область определения',
        'Muutumispiirkond': 'Область значений',
        'Paarsus': 'Чётность',
        'Nullkohad': 'Нули функции',
        'Märgipiirkonnad': 'Промежутки знакопостоянства',
        'Positiivsus- ja negatiivsuspiirkond': 'Промежутки положительности и отрицательности',
        'Esimene tuletis': 'Первая производная',
        'Kriitilised punktid': 'Критические точки',
        'Kasvamis- ja kahanemispiirkond': 'Промежутки возрастания и убывания',
        'Kasvamine ja kahanemine': 'Возрастание и убывание',
        'Ekstreemumid': 'Экстремумы',
        'Teine tuletis': 'Вторая производная',
        'Kumerus ja nõgusus': 'Выпуклость и вогнутость',
        'Käänupunktid': 'Точки перегиба',
        'Nullkoht': 'Нуль функции',
        'Maksimum': 'Максимум',
        'Miinimum': 'Минимум',
        'Käänupunkt': 'Точка перегиба',
        'Piiranguid pole (nimetaja, paarisjuur, logaritm puuduvad)': 'Ограничений нет (нет знаменателя, корня чётной степени и логарифма)',
        'Ühisosa': 'Пересечение',
        'Valitud lõik piirab määramispiirkonda': 'Выбранный отрезок ограничивает область определения',
        'Muutumispiirkonna leidmiseks võrdleme kriitiliste punktide ja otspunktide väärtusi ning avatud piiride piirväärtusi.': 'Для нахождения области значений сравниваем значения в критических точках и на концах промежутков, а также пределы на открытых границах.',
        'Määramispiirkond ei ole nulli suhtes sümmeetriline.': 'Область определения не симметрична относительно нуля.',
        'Nullfunktsioon on nii paaris kui ka paaritu.': 'Нулевая функция одновременно чётная и нечётная.',
        'Määramispiirkond on sümmeetriline ja f(-x) = f(x).': 'Область определения симметрична и f(-x) = f(x).',
        'Määramispiirkond on sümmeetriline ja f(-x) = -f(x).': 'Область определения симметрична и f(-x) = -f(x).',
        'Avaldis ei võrdu ei f(x) ega −f(x).': 'Выражение не равно ни f(x), ни −f(x).',
        'ei ole paaris ega paaritu': 'ни чётная, ни нечётная',
        'paaris ja paaritu (nullfunktsioon)': 'чётная и нечётная (нулевая функция)',
        'paaris': 'чётная',
        'paaritu': 'нечётная',
        'Määramispiirkond on tühi.': 'Область определения пуста.',
        'ei ole täielikult leitud': 'найдена не полностью',
        'Kogu muutumispiirkonna leidmiseks tuleb leida kõik kriitilised punktid ja piirväärtused.': 'Для нахождения всей области значений нужно найти все критические точки и пределы.',
        'Ühe piirväärtuse leidmine ei õnnestunud.': 'Не удалось найти один из пределов.',
        'Kui x →': 'При x →',
        'siis f(x) →': 'получаем f(x) →',
        'Märgitabel (testpunkt igas vahemikus)': 'Таблица знаков (проверочная точка в каждом промежутке)',
        'ei ole määratud (kuid': 'не определена (но',
        'on)': 'определена)',
        'ei ole määratud': 'не определена',
        'ei ole': 'не является',
        'sisepunkt – ei sobi.': 'внутренней точкой — не подходит.',
        'märgid': 'знаки',
        'Sama märgiga naabervahemikud ühendatakse': 'Соседние промежутки с одинаковым знаком объединяются',
        'märk ei muutu': 'знак не меняется',
        'Maksimumis muutub f′ märk plussist miinuseks, miinimumis miinusest plussiks.': 'В точке максимума знак f′ меняется с плюса на минус, в точке минимума — с минуса на плюс.',
        'ekstreemumit pole': 'экстремума нет',
        'käänupunkti pole': 'точки перегиба нет',
        'Lõigu otspunkti võrdleme lõigu sees asuvate väärtustega': 'Значение на конце отрезка сравниваем со значениями внутри отрезка',
        'f″ > 0: nõgus (∪); f″ < 0: kumer (∩).': 'f″ > 0: вогнута (∪); f″ < 0: выпукла (∩).',
        'kõik x ∈ X (funktsioon on konstantne)': 'все x ∈ X (функция постоянная)',
        'kõik x ∈ X': 'все x ∈ X',
        'puuduvad': 'отсутствуют',
        'puudub': 'не существует',
        'Leitud numbriliselt lõigul': 'Найдено численно на отрезке',
        'numbriliselt lõigul': 'численно на отрезке',
        'numbriliselt vahemikus': 'численно в диапазоне',
        'numbriliselt': 'численно',
        'ligikaudu': 'приближённо',
        'vahemikus': 'в диапазоне',
        'Samaselt null – kehtib iga': 'Тождественно ноль — верно для каждого',
        'korral.': '.',
        'Lahendid puuduvad.': 'Решений нет.',
        'lahendid puuduvad': 'решений нет',
        'Otsiti lõigul': 'Поиск на отрезке',
        'Lõigul': 'На отрезке',
        'Toome ühise teguri sulgude ette': 'Выносим общий множитель за скобки',
        'Lugeja on konstant': 'Числитель — постоянная',
        'Murd': 'Дробь',
        'lugeja': 'числитель',
        'Korrutis': 'Произведение',
        'kui mõni tegur': 'если один из множителей',
        'Aste': 'Степень',
        'alus': 'основание',
        'lugeja on 1': 'числитель равен 1',
        'alati.': 'всегда.',
        'Eksponent/juur ei saa olla negatiivne.': 'Экспонента или корень не могут быть отрицательными.',
        'Pöördfunktsiooniga': 'Используем обратную функцию',
        'nimetaja': 'знаменатель',
        'ei sobi': 'не подходит',
        'Algebraliselt ei lahendu': 'Не решается алгебраически',
        'Irratsionaalsed kordajad – juured': 'Иррациональные коэффициенты — корни',
        'vastused on ligikaudsed': 'ответы приближённые',
        'sulgude ette': 'выносим за скобки',
        'Ratsionaalne juur (vabaliikme jagajate seast)': 'Рациональный корень (среди делителей свободного члена)',
        'Horneri skeem, jagame': 'Схема Горнера, делим на',
        '-ga:': ':',
        'aste, ratsionaalseid juuri pole': 'степень, рациональных корней нет',
        'Ruut ei saa olla negatiivne – lahendid puuduvad.': 'Квадрат не может быть отрицательным — решений нет.',
        'Asendus': 'Замена',
        'või': 'или',
        'Konstandi tuletis': 'Производная постоянной',
        'Lihtsustatult': 'После упрощения',
        'Summa': 'Сумма',
        'Konstantne tegur': 'Постоянный множитель',
        'Jagatis': 'Частное',
        'Muutuja tuletis': 'Производная переменной',
        'Astmefunktsiooni tuletis': 'Производная степенной функции',
        'Liitfunktsiooni (astme) tuletis': 'Производная сложной степенной функции',
        'Eksponentfunktsiooni tuletis': 'Производная показательной функции',
        'Liitfunktsiooni (eksponendi) tuletis': 'Производная сложной показательной функции',
        'Muutuva astendajaga aste (logaritmiline diferentseerimine)': 'Степень с переменным показателем (логарифмическое дифференцирование)',
        'Tuletiste tabelist': 'По таблице производных',
        'Liitfunktsiooni tuletis (ahelreegel)': 'Производная сложной функции (правило цепочки)',
        'konstandi tuletis on 0': 'производная постоянной равна 0',
        'jagatise tuletis': 'производная частного',
        'korrutise tuletis': 'производная произведения',
        'konstantne tegur ette': 'выносим постоянный множитель',
        'muutuja tuletis on 1': 'производная переменной равна 1',
        'astme tuletis': 'производная степени',
        'liitfunktsiooni tuletis': 'производная сложной функции',
        'Paarisjuure alune': 'Подкоренное выражение корня чётной степени',
        'Logaritmi argument': 'Аргумент логарифма',
        'Nimetaja': 'Знаменатель',
        'juur nimetajas – juuritav': 'корень в знаменателе — подкоренное выражение',
        'muutuva astendajaga astme alus': 'основание степени с переменным показателем',
        'logaritmitav': 'аргумент логарифма',
        'Valem võib olla kuni 200 märki pikk.': 'Формула может содержать не более 200 символов.',
        'Vahemiku otspunkt peab olema lõigus [-1000; 1000].': 'Граница диапазона должна лежать в [-1000; 1000].',
        'Vahemiku otspunktid peavad olema lõigus [-1000; 1000].': 'Границы диапазона должны лежать в [-1000; 1000].',
        'Vahemiku algus peab olema väiksem kui lõpp ja vahemik vähemalt 0.01 pikkune.': 'Начало диапазона должно быть меньше конца, а его длина — не меньше 0.01.',
        'Valemi arvutamine ei õnnestunud. Kontrolli valemit.': 'Не удалось вычислить формулу. Проверьте её запись.',
        'Selle valemi tuletist ei õnnestunud leida – kontrolli valemi kuju.': 'Не удалось найти производную — проверьте запись формулы.',
        'Samm peab olema vähemalt 0.0001.': 'Шаг должен быть не меньше 0.0001.',
        'Graafikul võib olla kuni 20 000 sammu. Suurenda sammu.': 'На графике может быть не больше 20 000 шагов. Увеличьте шаг.',
        'Valem on tühi.': 'Формула пуста.',
        'Tundmatu märk': 'Неизвестный символ',
        'Tundmatu tähis': 'Неизвестное обозначение',
        'positsioon': 'позиция',
        'Muutuja peab olema x; lubatud funktsioonid': 'Переменная должна быть x; допустимые функции',
        'konstandid pi ja e.': 'константы pi и e.',
        'Liigne sulg': 'Лишняя скобка',
        'Ootamatu': 'Неожиданный символ',
        'Jagamine nulliga': 'Деление на ноль',
        'Aste ei ole reaalarvudes määratud': 'Степень не определена в действительных числах',
        'Puudub sulgev sulg': 'Отсутствует закрывающая скобка',
        'abs() ei ole toetatud: absoluutväärtusega funktsiooni ei saa siin uurida.': 'abs() не поддерживается: функции с модулем здесь не исследуются.',
        'Valem lõppes ootamatult': 'Формула неожиданно закончилась',
        'Funktsioonil puudub argument': 'У функции отсутствует аргумент',
        'Funktsioon ei ole selles punktis määratud': 'Функция не определена в этой точке',
        "Funktsiooni uurimine lihtsalt": "Исследование функции простыми словами",
        "11 teemat: mida leida, kuidas leida ja lihtne näide.": "11 тем: что искать, как найти и простой пример.",
        "Sisukord": "Содержание",
        "Näide": "Пример",
        "X näitab, milliseid x-väärtusi tohib valemisse panna. Y näitab, milliseid vastuseid võib saada.": "X — какие значения x можно подставлять в формулу. Y — какие ответы можно получить.",
        "Jagada nulliga ei tohi.": "На ноль делить нельзя.",
        "Ruutjuure all peab olema arv ≥ 0.": "Под квадратным корнем должно быть число ≥ 0.",
        "Logaritmi sees peab olema arv > 0.": "Под знаком логарифма должно быть число > 0.",
        "Y leidmiseks vaata väikseimat ja suurimat väärtust ning seda, millele funktsioon piiride lähedal läheneb.": "Для нахождения Y посмотрите наименьшее и наибольшее значения, а также к чему приближается функция около границ.",
        "x võib olla ükskõik milline arv. Kõige väiksem vastus on −4.": "x может быть любым числом. Самый маленький ответ — −4.",
        "Nurksulg [ ] tähendab, et piir kuulub vahemikku. Ümarsulg ( ) tähendab, et ei kuulu. Kui valid lõigu, uuri ainult selle sisse jäävaid x-väärtusi.": "Квадратная скобка [ ] означает, что граница входит в промежуток. Круглая ( ) — не входит. Если выбран отрезок, исследуйте только значения x внутри него, включая концы.",
        "Asenda valemis x väärtusega −x. Enne kontrolli, et nii x kui ka −x oleksid lubatud.": "Замените в формуле x на −x. Сначала проверьте, что и x, и −x допустимы.",
        "Kui f(−x) = f(x), on funktsioon paaris: graafik on y-telje suhtes peegelpildis.": "Если f(−x) = f(x), функция чётная: график зеркален относительно оси y.",
        "Kui f(−x) = −f(x), on funktsioon paaritu: graafik on alguspunkti suhtes sümmeetriline.": "Если f(−x) = −f(x), функция нечётная: график симметричен относительно начала координат.",
        "Kui kumbki võrdus ei kehti, pole funktsioon paaris ega paaritu.": "Если ни одно равенство не выполняется, функция ни чётная, ни нечётная.",
        "x² on paaris, x³ on paaritu. (x − 1)² ei ole kumbki.": "x² — чётная, x³ — нечётная. (x − 1)² — ни та ни другая.",
        "Nullkohas on vastus 0. Graafik puudutab või lõikab seal x-telge.": "В нуле функции ответ равен 0. В этом месте график касается оси x или пересекает её.",
        "Lahenda f(x) = 0. Jäta alles ainult lubatud x-väärtused, sealhulgas sobivad lõigu otspunktid.": "Решите f(x) = 0. Оставьте только допустимые значения x, включая подходящие концы отрезка.",
        "Murru korral pane lugeja võrduma nulliga; nimetaja ei tohi olla 0. Korrutis on 0, kui vähemalt üks tegur on 0.": "У дроби приравняйте числитель к нулю; знаменатель не должен быть равен 0. Произведение равно 0, если хотя бы один множитель равен 0.",
        "Positiivne tähendab f(x) > 0: graafik on x-teljest üleval. Negatiivne tähendab f(x) < 0: graafik on allpool.": "Положительная: f(x) > 0, график выше оси x. Отрицательная: f(x) < 0, график ниже оси x.",
        "Jaga määramispiirkond nullkohtade ja katkestuste juures osadeks. Proovi igast osast üht x-väärtust ja vaata vastuse märki. Nullkohad ise ei kuulu plussi ega miinuse alla.": "Разделите область определения нулями и точками разрыва. Подставьте по одному x из каждой части и посмотрите знак ответа. Сами нули не входят ни в плюс, ни в минус.",
        "x² − 4 on positiivne, kui x < −2 või x > 2. Vahemikus −2 < x < 2 on see negatiivne.": "x² − 4 положительна при x < −2 или x > 2. В промежутке −2 < x < 2 она отрицательна.",
        "Tuletis f′ näitab, kuidas funktsioon muutub. Leia see sobiva reegli järgi.": "Производная f′ показывает, как меняется функция. Найдите её по подходящему правилу.",
        "c on tavaline arv, mis ei sõltu x-ist.": "c — обычное число, которое не зависит от x.",
        "Summa, korrutise, murru ja liitfunktsiooni reeglid": "Правила для суммы, произведения, дроби и сложной функции",
        "u ja v tähistavad funktsioone. Liitfunktsioonis arvutad kõigepealt sees oleva avaldise ja siis välise funktsiooni.": "u и v обозначают функции. В сложной функции сначала вычисляется выражение внутри, затем внешняя функция.",
        "Liitfunktsiooni puhul leia välimise funktsiooni tuletis ja korruta see sisemise funktsiooni tuletisega.": "Для сложной функции найдите производную внешней функции и умножьте на производную внутренней.",
        "Leia määramispiirkonna sees kohad, kus f′(x) = 0 või tuletis puudub, kuid funktsioon ise on olemas.": "Найдите внутри области определения точки, где f′(x) = 0 или производной нет, но сама функция определена.",
        "Nimetaja nullkoht ei sobi, kui funktsioon on seal määramata. Vahemiku otspunkte kontrollitakse eraldi.": "Нуль знаменателя не подходит, если функция там не определена. Концы промежутка проверяются отдельно.",
        "Vaata esimese tuletise märki: f′ > 0 tähendab kasvamist, f′ < 0 kahanemist.": "Посмотрите знак первой производной: f′ > 0 — функция возрастает, f′ < 0 — убывает.",
        "Kontrolli märki kriitiliste punktide vahel. Katkestuse eri pooltel olevaid vahemikke ei tohi ühendada.": "Проверьте знак между критическими точками. Через точку разрыва промежутки объединять нельзя.",
        "x² − 4 kahaneb, kui x < 0, ja kasvab, kui x > 0.": "x² − 4 убывает при x < 0 и возрастает при x > 0.",
        "Maksimum on kohalik tipp, miinimum kohalik põhi.": "Максимум — местная вершина графика, минимум — местная впадина.",
        "f′ muudab märki + → −: maksimum.": "f′ меняет знак + → −: максимум.",
        "f′ muudab märki − → +: miinimum.": "f′ меняет знак − → +: минимум.",
        "Kui märk ei muutu, pole selles kohas ekstreemumit.": "Если знак не меняется, экстремума в этом месте нет.",
        "Pane leitud x algsesse valemisse, et saada y. Kinnisel lõigul võrdle ka otspunktide väärtusi.": "Подставьте найденный x в исходную формулу, чтобы получить y. На замкнутом отрезке сравните также значения на концах.",
        "Funktsiooni x² − 4 miinimumpunkt on (0; −4).": "У функции x² − 4 точка минимума — (0; −4).",
        "Leia esimese tuletise tuletis. Kasuta samu reegleid veel kord.": "Возьмите производную от первой производной. Примените те же правила ещё раз.",
        "Teise tuletise märk näitab, kumba pidi graafik paindub.": "Знак второй производной показывает, в какую сторону изгибается график.",
        "f″ > 0: kuju ∪, nõgus.": "f″ > 0: форма ∪, график вогнутый.",
        "f″ < 0: kuju ∩, kumer.": "f″ < 0: форма ∩, график выпуклый.",
        "Jaga määramispiirkond f″ nullkohtade ja määramata kohtade juures osadeks ning kontrolli igas osas märki.": "Разделите область определения нулями f″ и точками, где f″ не существует. В каждой части проверьте знак.",
        "Funktsioonil x² − 4 on f″ = 2 > 0, seega on graafiku kuju ∪.": "Для x² − 4 получаем f″ = 2 > 0, поэтому график имеет форму ∪.",
        "Käänupunktis muutub graafiku paindumise suund: ∪ läheb üle kujuks ∩ või vastupidi.": "В точке перегиба меняется направление изгиба: форма ∪ переходит в ∩ или наоборот.",
        "Leia kohad, kus f″ = 0 või f″ puudub. Kontrolli, kas f″ märk muutub. Graafik ei tohi selles kohas katkeda.": "Найдите точки, где f″ = 0 или f″ не существует. Проверьте смену знака f″. График в этом месте не должен разрываться.",
        "Ainult f″ = 0 ei ole piisav. Punkti koordinaatide jaoks leia ka y = f(x).": "Одного равенства f″ = 0 недостаточно. Для координат точки найдите также y = f(x).",
        "Vasakul on f″ negatiivne, paremal positiivne. Seega märk muutub.": "Слева f″ отрицательна, справа положительна. Значит, знак меняется."
    };
    const lookup = new Map(Object.entries(ru).map(([et, translation]) => [et.toLowerCase(), translation]));
    const escape = text => text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const phrases = new RegExp(`(?<![\\p{L}])(?:${[...lookup.keys()].sort((a,b)=>b.length-a.length).map(escape).join('|')})(?![\\p{L}])`, 'giu');
    function translate(text) {
        if (document.documentElement.lang !== 'ru') return text;
        return text.replace(phrases, match => lookup.get(match.toLowerCase()));
    }
    window.appI18n = {
        text: translate,
        latex: text => text.replace(/\\text\{([^{}]*)\}/g, (_, content) => `\\text{${translate(content)}}`)
    };
    const texts = new WeakMap(), attributes = new WeakMap();
    const ignored = 'script, style, code, textarea, .katex, [data-latex], [data-no-translate]';
    const translatableAttributes = ['aria-label', 'title', 'placeholder', 'alt', 'label'];
    function updateText(node) {
        if (!node.parentElement || node.parentElement.closest(ignored)) return;
        const value = node.nodeValue;
        let record = texts.get(node);
        if (!record || value !== record.rendered) record = { source: value };
        record.rendered = translate(record.source);
        texts.set(node, record);
        if (value !== record.rendered) node.nodeValue = record.rendered;
    }
    function updateAttributes(element) {
        if (element.closest(ignored)) return;
        let records = attributes.get(element);
        if (!records) { records = {}; attributes.set(element, records); }
        for (const attribute of translatableAttributes) {
            const value = element.getAttribute(attribute);
            if (value == null) continue;
            let record = records[attribute];
            if (!record || value !== record.rendered) record = { source: value };
            record.rendered = translate(record.source);
            records[attribute] = record;
            if (value !== record.rendered) element.setAttribute(attribute, record.rendered);
        }
    }
    function visit(root) {
        if (root.nodeType === Node.TEXT_NODE) { updateText(root); return; }
        if (root.nodeType !== Node.ELEMENT_NODE || root.closest(ignored)) return;
        updateAttributes(root);
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_ELEMENT | NodeFilter.SHOW_TEXT, {
            acceptNode: node => node.nodeType === Node.ELEMENT_NODE && node.matches(ignored) ? NodeFilter.FILTER_REJECT : NodeFilter.FILTER_ACCEPT
        });
        let node;
        while ((node = walker.nextNode())) {
            if (node.nodeType === Node.TEXT_NODE) updateText(node); else updateAttributes(node);
        }
    }
    const observer = new MutationObserver(records => {
        observer.disconnect();
        for (const record of records) {
            if (record.type === 'childList') record.addedNodes.forEach(visit);
            else if (record.type === 'characterData') updateText(record.target);
            else updateAttributes(record.target);
        }
        observe();
    });
    const observe = () => observer.observe(document.body, { childList: true, subtree: true, characterData: true, attributes: true, attributeFilter: translatableAttributes });
    const sourceTitle = document.title;
    function applyLanguage() {
        observer.disconnect();
        visit(document.body);
        document.title = translate(sourceTitle);
        document.querySelector('[data-language]').value = document.documentElement.lang;
        window.renderMath?.();
        observe();
    }
    window.addEventListener('app-language-change', applyLanguage);
    document.querySelector('[data-language]').addEventListener('change', event => appPreferences.setLanguage(event.target.value));
    applyLanguage();
})();
