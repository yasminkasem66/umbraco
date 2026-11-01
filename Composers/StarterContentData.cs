namespace UmbracoDemo.Composers;

/// <summary>
/// The editorial copy behind the starter content seeded by StarterContentSeedingComposer.cs —
/// kept apart from the seeding plumbing so the content can be read and edited like copy rather
/// than code. Every organisation, person, and figure below is fictional; the photos they
/// reference live in SeedData/images (see SeedData/images/CREDITS.md).
/// </summary>
public static class StarterContentData
{
    public const string SiteNameEn = "Delta News";
    public const string SiteNameAr = "أخبار الدلتا";

    public sealed record CategorySeed(string En, string Ar, string Slug, string Colour);

    public sealed record LocalizedArticle(string Title, string Summary, string BodyHtml, string Author, string[] Tags);

    public sealed record ArticleSeed(
        string Slug,
        string Category,
        string Image,
        string[] Gallery,
        int DaysAgo,
        bool Featured,
        int FeaturedOrder,
        LocalizedArticle En,
        LocalizedArticle? Ar);

    public static readonly CategorySeed[] Categories =
    [
        new("Technology", "تقنية", "technology", "#2563eb"),
        new("Sports", "رياضة", "sports", "#16a34a"),
        new("Business", "أعمال", "business", "#d97706"),
        new("Health", "صحة", "health", "#dc2626"),
        new("Culture", "ثقافة", "culture", "#7c3aed"),
    ];

    public static readonly ArticleSeed[] Articles =
    [
        new(
            Slug: "public-hospitals-roll-out-online-appointment-booking",
            Category: "Health",
            Image: "hospital-online-booking.jpg",
            Gallery: [],
            DaysAgo: 0,
            Featured: true,
            FeaturedOrder: 4,
            En: new(
                "Public Hospitals Roll Out Online Appointment Booking",
                "Patients at the region's nine public hospitals can now book outpatient appointments online or by text message, ending the early-morning queues that have long defined a visit to the clinic.",
                """
                <p>Patients at the Delta region's nine public hospitals can now book outpatient appointments online or by text message, in a change health officials say will end the early-morning queues that have defined a hospital visit for decades.</p>
                <p>The new service, launched on Sunday, lets patients choose a clinic, a doctor and a 15-minute time slot up to three weeks ahead. A confirmation code arrives by SMS and is scanned at a kiosk on arrival, replacing the paper ticket system.</p>
                <h2>Pilot cut waiting times by half</h2>
                <p>During a six-month pilot at two hospitals, the average wait between arrival and consultation fell from 2 hours 40 minutes to 70 minutes, according to the regional health directorate. Missed appointments also dropped after the system began sending reminders the evening before.</p>
                <blockquote><p>"The queue at six in the morning was never a medical necessity. It was an information problem, and this solves it," said Dr Hala Mansour, who led the pilot.</p></blockquote>
                <p>Walk-in patients will still be seen, and emergency departments are unaffected. A telephone line staffed by booking clerks is available for patients without a smartphone.</p>
                """,
                "Nour El-Sayed",
                ["health", "hospitals", "digital services"]),
            Ar: new(
                "المستشفيات العامة تطلق خدمة حجز المواعيد إلكترونيًا",
                "أصبح بإمكان المرضى في المستشفيات العامة التسعة بالإقليم حجز مواعيد العيادات الخارجية عبر الإنترنت أو الرسائل النصية، لتنتهي بذلك طوابير الصباح الباكر أمام العيادات.",
                """
                <p>أصبح بإمكان المرضى في المستشفيات العامة التسعة بإقليم الدلتا حجز مواعيد العيادات الخارجية عبر الإنترنت أو الرسائل النصية، في خطوة يقول مسؤولو الصحة إنها ستنهي طوابير الصباح الباكر التي ارتبطت بزيارة المستشفى لعقود.</p>
                <p>وتتيح الخدمة الجديدة، التي انطلقت يوم الأحد، اختيار العيادة والطبيب وموعد مدته ١٥ دقيقة قبل ما يصل إلى ثلاثة أسابيع. ويصل رمز التأكيد برسالة نصية ويُمسح ضوئيًا عند الوصول بدلًا من نظام التذاكر الورقية.</p>
                <h2>التجربة خفّضت وقت الانتظار إلى النصف</h2>
                <p>وخلال تجربة استمرت ستة أشهر في مستشفيين، انخفض متوسط الانتظار من ساعتين و٤٠ دقيقة إلى ٧٠ دقيقة، بحسب مديرية الصحة بالإقليم.</p>
                <blockquote><p>«طابور السادسة صباحًا لم يكن ضرورة طبية قط، بل كان مشكلة معلومات، وهذا النظام يحلها»، كما قالت الدكتورة هالة منصور التي قادت التجربة.</p></blockquote>
                <p>وسيظل استقبال المرضى دون موعد متاحًا، ولن تتأثر أقسام الطوارئ، كما يتوفر خط هاتفي للحجز لمن لا يملكون هاتفًا ذكيًا.</p>
                """,
                "نور السيد",
                ["صحة", "مستشفيات", "خدمات رقمية"])),

        new(
            Slug: "city-unveils-new-tech-hub",
            Category: "Technology",
            Image: "city-tech-hub.jpg",
            Gallery: ["city-tech-hub-workspace.jpg", "city-tech-hub-desk.jpg"],
            DaysAgo: 1,
            Featured: true,
            FeaturedOrder: 1,
            En: new(
                "City Unveils New Tech Hub",
                "A converted textile warehouse on the riverfront opens this week as the city's largest technology hub, with 600 desks, a hardware lab and subsidised space for early-stage founders.",
                """
                <p>A converted textile warehouse on the riverfront opened its doors on Monday as the city's largest technology hub, with room for 600 people across four floors and a waiting list that already runs to more than 80 companies.</p>
                <p>The Riverside Works hub was funded jointly by the municipality and a group of local investors at a cost of 210 million pounds. A third of its desks are reserved for companies less than two years old, which will pay a quarter of the market rent for their first year.</p>
                <h2>What's inside</h2>
                <ul>
                  <li>A hardware lab with 3D printers, a laser cutter and an electronics bench</li>
                  <li>Twelve meeting rooms and a 250-seat event hall</li>
                  <li>A ground-floor café and childcare room open to all tenants</li>
                </ul>
                <blockquote><p>"We kept losing our best graduates to other cities because there was nowhere here to build a company. That changes today," said Karim Adel, the hub's director.</p></blockquote>
                <p>The first cohort of 40 start-ups moves in next month, with a public open day planned for the first Saturday of each month.</p>
                """,
                "Mariam Fawzy",
                ["technology", "startups", "innovation"]),
            Ar: new(
                "المدينة تفتتح مركزًا جديدًا للتكنولوجيا",
                "يفتتح مستودع نسيج سابق على ضفاف النهر أبوابه هذا الأسبوع كأكبر مركز للتكنولوجيا في المدينة، ويضم ٦٠٠ مكتب ومختبرًا للأجهزة ومساحات مدعومة للشركات الناشئة.",
                """
                <p>افتتح مستودع نسيج سابق على ضفاف النهر أبوابه يوم الاثنين كأكبر مركز للتكنولوجيا في المدينة، بسعة ٦٠٠ شخص على أربعة طوابق، وقائمة انتظار تضم بالفعل أكثر من ٨٠ شركة.</p>
                <p>ومُوّل مركز «ريفرسايد ووركس» بالشراكة بين البلدية ومجموعة من المستثمرين المحليين بتكلفة ٢١٠ ملايين جنيه. وخُصص ثلث المكاتب للشركات التي لم يمضِ على تأسيسها عامان، والتي ستدفع ربع الإيجار السوقي في عامها الأول.</p>
                <h2>ماذا يضم المركز؟</h2>
                <ul>
                  <li>مختبر للأجهزة يضم طابعات ثلاثية الأبعاد وآلة قطع بالليزر</li>
                  <li>اثنتا عشرة قاعة اجتماعات وقاعة فعاليات تتسع لـ٢٥٠ شخصًا</li>
                  <li>مقهى وغرفة لرعاية الأطفال متاحة لجميع الشركات</li>
                </ul>
                <blockquote><p>«كنا نخسر أفضل خريجينا لصالح مدن أخرى لأنه لم يكن هناك مكان لبناء شركة. هذا يتغير اليوم»، كما قال كريم عادل مدير المركز.</p></blockquote>
                """,
                "مريم فوزي",
                ["تكنولوجيا", "شركات ناشئة", "ابتكار"])),

        new(
            Slug: "local-team-wins-championship",
            Category: "Sports",
            Image: "championship-stadium.jpg",
            Gallery: ["championship-fans.jpg"],
            DaysAgo: 2,
            Featured: true,
            FeaturedOrder: 2,
            En: new(
                "Local Team Wins Championship",
                "The Delta Falcons beat Harbour City 2-1 in front of a sold-out home crowd on Friday night, sealing the club's first league title in eleven years with a stoppage-time header.",
                """
                <p>The Delta Falcons are league champions for the first time in eleven years after a stoppage-time header from captain Youssef Hamdy sealed a 2-1 win over Harbour City in front of a sold-out crowd of 32,000 on Friday night.</p>
                <p>Harbour City took the lead against the run of play just before half-time, but the Falcons equalised through 19-year-old winger Adam Nabil on 63 minutes. With the match heading for a draw that would have taken the title race to the final weekend, Hamdy rose at the far post to meet a corner in the 93rd minute.</p>
                <blockquote><p>"I've waited my whole career for that moment, and I got to share it with the people I grew up with," Hamdy said.</p></blockquote>
                <p>The win puts the Falcons seven points clear with one game remaining. The club will parade the trophy through the city centre on Sunday afternoon, with the route to be confirmed by police on Saturday.</p>
                """,
                "Omar Khaled",
                ["football", "league", "delta falcons"]),
            Ar: new(
                "الفريق المحلي يتوج بلقب الدوري",
                "فاز فريق صقور الدلتا على هاربور سيتي ٢-١ أمام مدرجات ممتلئة مساء الجمعة، ليحسم أول لقب للدوري منذ إحدى عشرة سنة برأسية في الوقت بدل الضائع.",
                """
                <p>توّج فريق صقور الدلتا بلقب الدوري للمرة الأولى منذ إحدى عشرة سنة، بعدما حسمت رأسية القائد يوسف حمدي في الوقت بدل الضائع الفوز على هاربور سيتي ٢-١ أمام ٣٢ ألف متفرج مساء الجمعة.</p>
                <p>وتقدم هاربور سيتي قبل نهاية الشوط الأول بقليل، لكن الجناح الشاب آدم نبيل (١٩ عامًا) أدرك التعادل في الدقيقة ٦٣، قبل أن يرتقي حمدي لركلة ركنية في الدقيقة ٩٣.</p>
                <blockquote><p>«انتظرت هذه اللحظة طوال مسيرتي، وتقاسمتها مع الناس الذين كبرت بينهم»، قال حمدي.</p></blockquote>
                <p>وسيطوف الفريق بالكأس في وسط المدينة بعد ظهر الأحد.</p>
                """,
                "عمر خالد",
                ["كرة القدم", "الدوري", "صقور الدلتا"])),

        new(
            Slug: "startup-raises-seed-funding",
            Category: "Business",
            Image: "startup-seed-funding.jpg",
            Gallery: [],
            DaysAgo: 3,
            Featured: true,
            FeaturedOrder: 3,
            En: new(
                "Startup Raises Seed Funding",
                "Freight-matching start-up Mersal has raised $4.5 million in seed funding to connect small manufacturers with independent truck owners, and plans to double its team by the summer.",
                """
                <p>Mersal, a two-year-old start-up that matches small manufacturers with independent truck owners, has raised $4.5 million in a seed round led by regional fund Crescent Ventures.</p>
                <p>The company says around 40 per cent of trucks on the region's roads travel empty on their return journey. Its app lets factories post loads and lets drivers bid on them, with payment held in escrow until delivery is confirmed by photo.</p>
                <h2>From 30 drivers to 2,800</h2>
                <p>Co-founders Salma Ibrahim and Tarek Fahmy began by phoning drivers from a shared desk in 2024. The platform now has 2,800 registered drivers and moved 61,000 loads last year.</p>
                <blockquote><p>"Small factories were paying for half-empty trucks because they had no way to find a return load. That's money that should be going into wages," Ibrahim said.</p></blockquote>
                <p>The new funding will pay for an expansion into refrigerated freight and grow the team from 35 to 70 people by the summer.</p>
                """,
                "Hany Mostafa",
                ["startups", "funding", "logistics"]),
            Ar: new(
                "شركة ناشئة تجمع تمويلًا تأسيسيًا",
                "جمعت شركة «مِرسال» الناشئة لمطابقة الشحنات ٤٫٥ مليون دولار في جولة تمويل تأسيسية، لربط المصانع الصغيرة بأصحاب الشاحنات المستقلين، وتخطط لمضاعفة فريقها بحلول الصيف.",
                """
                <p>جمعت شركة «مِرسال»، وهي شركة ناشئة عمرها عامان تربط المصانع الصغيرة بأصحاب الشاحنات المستقلين، ٤٫٥ مليون دولار في جولة تمويل تأسيسية قادها صندوق «كريسنت فنتشرز» الإقليمي.</p>
                <p>وتقول الشركة إن نحو ٤٠ في المئة من الشاحنات تعود فارغة من رحلاتها. ويتيح تطبيقها للمصانع نشر الشحنات وللسائقين التقدم بعروض عليها، مع حفظ المبلغ لدى طرف ثالث حتى تأكيد التسليم بصورة.</p>
                <blockquote><p>«كانت المصانع الصغيرة تدفع ثمن شاحنات نصف فارغة، وهذه أموال يجب أن تذهب إلى الأجور»، قالت الشريكة المؤسسة سلمى إبراهيم.</p></blockquote>
                <p>وسيُستخدم التمويل الجديد للتوسع في الشحن المبرّد وزيادة الفريق من ٣٥ إلى ٧٠ موظفًا.</p>
                """,
                "هاني مصطفى",
                ["شركات ناشئة", "تمويل", "خدمات لوجستية"])),

        new(
            Slug: "ai-adoption-grows-among-small-businesses",
            Category: "Technology",
            Image: "ai-small-business.jpg",
            Gallery: [],
            DaysAgo: 4,
            Featured: false,
            FeaturedOrder: 0,
            En: new(
                "AI Adoption Grows Among Small Businesses",
                "More than a third of small businesses now use at least one AI tool, up from 17 per cent a year ago, according to a survey of 1,200 firms by the Delta Chamber of Commerce.",
                """
                <p>More than a third of small businesses in the region now use at least one artificial intelligence tool, more than double the share a year ago, according to a survey of 1,200 firms published on Wednesday by the Delta Chamber of Commerce.</p>
                <p>Thirty-eight per cent of respondents said they used AI for tasks such as drafting marketing copy, answering customer messages or reconciling invoices, compared with 17 per cent in last year's survey.</p>
                <h2>Cost and skills remain barriers</h2>
                <p>Among firms not using AI, the most common reasons given were uncertainty about where to start (44 per cent), concerns about customer data (31 per cent) and cost (22 per cent). Only one in ten said they had received any formal training.</p>
                <p>The chamber said it would run free half-day workshops for members from next month, starting with retailers and restaurants.</p>
                """,
                "Mariam Fawzy",
                ["technology", "artificial intelligence", "small business"]),
            Ar: new(
                "تزايد استخدام الذكاء الاصطناعي في الشركات الصغيرة",
                "أصبح أكثر من ثلث الشركات الصغيرة يستخدم أداة ذكاء اصطناعي واحدة على الأقل، مقارنة بـ١٧ في المئة قبل عام، وفق مسح شمل ١٢٠٠ شركة أجرته غرفة تجارة الدلتا.",
                """
                <p>أصبح أكثر من ثلث الشركات الصغيرة في الإقليم يستخدم أداة واحدة على الأقل من أدوات الذكاء الاصطناعي، أي أكثر من ضعف النسبة قبل عام، وفق مسح شمل ١٢٠٠ شركة نشرته غرفة تجارة الدلتا يوم الأربعاء.</p>
                <p>وقال ٣٨ في المئة من المشاركين إنهم يستخدمون الذكاء الاصطناعي في مهام مثل كتابة النصوص التسويقية والرد على رسائل العملاء ومطابقة الفواتير.</p>
                <h2>التكلفة والمهارات لا تزال عائقًا</h2>
                <p>وأرجعت الشركات التي لا تستخدم هذه الأدوات ذلك إلى عدم معرفة نقطة البداية (٤٤٪) والمخاوف بشأن بيانات العملاء (٣١٪) والتكلفة (٢٢٪). وستنظم الغرفة ورش عمل مجانية لأعضائها اعتبارًا من الشهر المقبل.</p>
                """,
                "مريم فوزي",
                ["تكنولوجيا", "ذكاء اصطناعي", "شركات صغيرة"])),

        new(
            Slug: "clinics-report-early-start-to-flu-season",
            Category: "Health",
            Image: "flu-season.jpg",
            Gallery: [],
            DaysAgo: 5,
            Featured: false,
            FeaturedOrder: 0,
            En: new(
                "Clinics Report Early Start to Flu Season",
                "Family clinics are seeing flu cases three weeks earlier than usual this year. Doctors are urging older adults, pregnant women and people with chronic conditions to get vaccinated now.",
                """
                <p>Family clinics across the region are reporting a rise in flu cases around three weeks earlier than in a typical year, prompting doctors to urge people in higher-risk groups to get vaccinated now rather than waiting.</p>
                <p>Consultations for flu-like illness rose by 60 per cent over the past fortnight, according to figures shared by the regional family medicine network. Most cases have been mild, but clinics have also seen an increase in children with high fevers.</p>
                <h2>What doctors recommend</h2>
                <ul>
                  <li>Get the seasonal flu vaccine, especially if you are over 65, pregnant, or have a chronic condition such as diabetes, asthma or heart disease</li>
                  <li>Wash hands often and cover coughs and sneezes</li>
                  <li>Stay home while you have a fever, and seek medical advice if symptoms are severe or last more than a few days</li>
                </ul>
                <p>The vaccine is available free of charge at public family clinics for people in priority groups.</p>
                """,
                "Nour El-Sayed",
                ["health", "flu", "vaccination"]),
            Ar: new(
                "العيادات تسجل بداية مبكرة لموسم الإنفلونزا",
                "تستقبل عيادات طب الأسرة حالات الإنفلونزا هذا العام قبل موعدها المعتاد بثلاثة أسابيع، ويحث الأطباء كبار السن والحوامل وأصحاب الأمراض المزمنة على التطعيم الآن.",
                """
                <p>تسجل عيادات طب الأسرة في الإقليم ارتفاعًا في حالات الإنفلونزا قبل نحو ثلاثة أسابيع من موعدها المعتاد، ما دفع الأطباء إلى حث الفئات الأكثر عرضة للخطر على التطعيم الآن.</p>
                <p>وارتفعت الاستشارات المتعلقة بأعراض شبيهة بالإنفلونزا بنسبة ٦٠ في المئة خلال الأسبوعين الماضيين، ومعظم الحالات خفيفة.</p>
                <h2>توصيات الأطباء</h2>
                <ul>
                  <li>تلقي لقاح الإنفلونزا الموسمية، خاصة لمن تجاوزوا ٦٥ عامًا والحوامل وأصحاب الأمراض المزمنة</li>
                  <li>غسل اليدين باستمرار وتغطية الفم عند السعال والعطس</li>
                  <li>البقاء في المنزل أثناء الحمى وطلب المشورة الطبية إذا اشتدت الأعراض</li>
                </ul>
                <p>ويتوفر اللقاح مجانًا في عيادات طب الأسرة العامة للفئات ذات الأولوية.</p>
                """,
                "نور السيد",
                ["صحة", "إنفلونزا", "تطعيم"])),

        new(
            Slug: "marathon-draws-record-crowds",
            Category: "Sports",
            Image: "marathon-runner.jpg",
            Gallery: ["marathon-crowd.jpg", "marathon-street.jpg"],
            DaysAgo: 6,
            Featured: false,
            FeaturedOrder: 0,
            En: new(
                "Marathon Draws Record Crowds",
                "A record 14,000 runners took part in Saturday's city marathon, while an estimated 90,000 spectators lined the riverside route. The women's course record fell for the first time since 2019.",
                """
                <p>A record 14,000 runners crossed the start line of the city marathon on Saturday morning, cheered on by an estimated 90,000 spectators along a route that follows the river for most of its 42.2 kilometres.</p>
                <p>Rana Saleh won the women's race in 2:31:08, taking more than a minute off a course record that had stood since 2019. Ahmed Ragab retained his men's title in 2:14:52.</p>
                <p>Organisers said the 5 km fun run, added two years ago, accounted for much of the growth, with nearly 6,000 entrants, a third of them taking part in their first organised race.</p>
                <blockquote><p>"Watching the families come through the finish together was the best part of the day," said race director Mona Gaber.</p></blockquote>
                <p>Entries for next year's race open on 1 February.</p>
                """,
                "Omar Khaled",
                ["running", "marathon", "community"]),
            Ar: new(
                "الماراثون يجذب حشودًا قياسية",
                "شارك رقم قياسي بلغ ١٤ ألف عدّاء في ماراثون المدينة يوم السبت، واصطف نحو ٩٠ ألف متفرج على طول المسار المحاذي للنهر، وسقط الرقم القياسي للسيدات لأول مرة منذ ٢٠١٩.",
                """
                <p>شارك رقم قياسي بلغ ١٤ ألف عدّاء في ماراثون المدينة صباح السبت، وسط تشجيع نحو ٩٠ ألف متفرج على طول مسار يحاذي النهر في معظم كيلومتراته الـ٤٢٫٢.</p>
                <p>وفازت رنا صالح بسباق السيدات بزمن ٢:٣١:٠٨، محطمة رقمًا قياسيًا صمد منذ ٢٠١٩، فيما احتفظ أحمد رجب بلقب الرجال بزمن ٢:١٤:٥٢.</p>
                <p>وقال المنظمون إن سباق الخمسة كيلومترات للهواة استقطب نحو ستة آلاف مشارك، ثلثهم يخوضون سباقهم المنظم الأول. ويُفتح باب التسجيل لسباق العام المقبل في الأول من فبراير.</p>
                """,
                "عمر خالد",
                ["جري", "ماراثون", "مجتمع"])),

        new(
            Slug: "regional-bank-reports-strong-quarter",
            Category: "Business",
            Image: "regional-bank.jpg",
            Gallery: [],
            DaysAgo: 7,
            Featured: false,
            FeaturedOrder: 0,
            En: new(
                "Regional Bank Reports Strong Quarter",
                "Riverside Commercial Bank posted an 18 per cent rise in third-quarter net profit, driven by lending to small and medium-sized businesses and a jump in customers using its mobile app.",
                """
                <p>Riverside Commercial Bank reported an 18 per cent rise in third-quarter net profit on Tuesday, beating analysts' expectations on the back of strong lending to small and medium-sized businesses.</p>
                <p>Net profit reached 1.42 billion pounds for the three months to September. The bank's loan book to smaller firms grew by 11 per cent over the quarter, while non-performing loans held steady at 3.1 per cent of the total.</p>
                <p>Chief executive Laila Hassan said more than 60 per cent of retail customers now use the bank's mobile app at least once a month, up from 45 per cent a year earlier, allowing it to close four branches without losing customers.</p>
                <p>Shares in the bank rose 4 per cent in early trading.</p>
                """,
                "Hany Mostafa",
                ["banking", "earnings", "economy"]),
            Ar: null),

        new(
            Slug: "book-fair-returns-with-400-publishers",
            Category: "Culture",
            Image: "book-fair.jpg",
            Gallery: ["book-fair-pages.jpg", "book-fair-stand.jpg"],
            DaysAgo: 8,
            Featured: false,
            FeaturedOrder: 0,
            En: new(
                "Book Fair Returns With 400 Publishers",
                "The Delta Book Fair opens on Thursday with 400 publishers from 22 countries, a new children's pavilion and a programme of more than 120 talks, readings and signings over ten days.",
                """
                <p>The Delta Book Fair returns to the exhibition grounds on Thursday with 400 publishers from 22 countries, its largest line-up since the fair was founded.</p>
                <p>This year's edition adds a dedicated children's pavilion with daily storytelling sessions, and a translation corner where visitors can meet translators working between Arabic, English, French and Turkish.</p>
                <h2>Highlights</h2>
                <ul>
                  <li>More than 120 talks, readings and book signings over ten days</li>
                  <li>A second-hand book market on both Fridays</li>
                  <li>Free entry for students and visitors over 60</li>
                </ul>
                <p>Organisers expect more than a million visitors. The fair is open daily from 10 am to 9 pm, and a free shuttle bus will run from the central station.</p>
                """,
                "Yasmine Adly",
                ["books", "culture", "events"]),
            Ar: new(
                "معرض الكتاب يعود بمشاركة ٤٠٠ ناشر",
                "يفتتح معرض الدلتا للكتاب أبوابه يوم الخميس بمشاركة ٤٠٠ ناشر من ٢٢ دولة، مع جناح جديد للأطفال وبرنامج يضم أكثر من ١٢٠ ندوة وأمسية وحفل توقيع على مدى عشرة أيام.",
                """
                <p>يعود معرض الدلتا للكتاب إلى أرض المعارض يوم الخميس بمشاركة ٤٠٠ ناشر من ٢٢ دولة، في أكبر دورة منذ تأسيسه.</p>
                <p>وتضيف دورة هذا العام جناحًا مخصصًا للأطفال يقدم جلسات حكي يومية، وركنًا للترجمة يلتقي فيه الزوار مترجمين يعملون بين العربية والإنجليزية والفرنسية والتركية.</p>
                <h2>أبرز الفعاليات</h2>
                <ul>
                  <li>أكثر من ١٢٠ ندوة وأمسية وحفل توقيع على مدى عشرة أيام</li>
                  <li>سوق للكتب المستعملة يومي الجمعة</li>
                  <li>دخول مجاني للطلاب ولمن تجاوزوا الستين</li>
                </ul>
                <p>ويفتح المعرض يوميًا من العاشرة صباحًا حتى التاسعة مساءً، مع حافلة مجانية من المحطة المركزية.</p>
                """,
                "ياسمين عدلي",
                ["كتب", "ثقافة", "فعاليات"])),

        new(
            Slug: "new-data-centre-opens-downtown",
            Category: "Technology",
            Image: "data-centre.jpg",
            Gallery: [],
            DaysAgo: 9,
            Featured: false,
            FeaturedOrder: 0,
            En: new(
                "New Data Centre Opens Downtown",
                "A 12-megawatt data centre has opened in a refurbished office tower downtown, promising faster cloud services for local companies and reusing its waste heat to warm the building next door.",
                """
                <p>A 12-megawatt data centre has opened inside a refurbished 1970s office tower downtown, the first facility of its size in the city centre.</p>
                <p>Operator Nilestack says hosting servers closer to users will cut response times for local banks, retailers and government services that currently rely on facilities hundreds of kilometres away.</p>
                <p>The building is cooled using treated river water, and waste heat from the servers is piped to a neighbouring hotel to heat water for its laundry and kitchens. The company expects the system to save around 3,000 tonnes of carbon dioxide a year.</p>
                <p>The centre employs 45 permanent staff and is already 70 per cent leased, according to the company.</p>
                """,
                "Mariam Fawzy",
                ["technology", "cloud", "infrastructure"]),
            Ar: null),

        new(
            Slug: "restored-cinema-reopens-with-classic-film-week",
            Category: "Culture",
            Image: "cinema-reopens.jpg",
            Gallery: [],
            DaysAgo: 10,
            Featured: false,
            FeaturedOrder: 0,
            En: new(
                "Restored Cinema Reopens With Classic Film Week",
                "The Art Deco Rialto cinema reopens after a four-year restoration, marking the occasion with a week of classic films projected on the venue's original 35mm projectors.",
                """
                <p>The Rialto, the city's last surviving Art Deco cinema, reopens on Wednesday after a four-year restoration that returned its painted ceiling, velvet seats and neon façade to their 1938 condition.</p>
                <p>To mark the reopening, the cinema will screen a week of classic films on its two restored 35mm projectors, alongside a modern digital system installed for new releases.</p>
                <blockquote><p>"People told us a single-screen cinema couldn't survive. We think a place this beautiful is a reason to go out," said programme manager Dina Fouad.</p></blockquote>
                <p>Tickets for the classic film week cost 50 pounds, with all proceeds going towards a fund to restore the cinema's original organ.</p>
                """,
                "Yasmine Adly",
                ["cinema", "heritage", "film"]),
            Ar: new(
                "سينما تاريخية تعود بعد الترميم بأسبوع للأفلام الكلاسيكية",
                "تعيد سينما «ريالتو» ذات الطراز الآرت ديكو فتح أبوابها بعد ترميم استمر أربع سنوات، وتحتفل بأسبوع من الأفلام الكلاسيكية تُعرض بأجهزة العرض الأصلية مقاس ٣٥ ملم.",
                """
                <p>تعيد سينما «ريالتو»، آخر دار عرض باقية على طراز الآرت ديكو في المدينة، فتح أبوابها يوم الأربعاء بعد ترميم استمر أربع سنوات أعاد سقفها المرسوم ومقاعدها المخملية وواجهتها المضيئة إلى حالتها عام ١٩٣٨.</p>
                <p>وبهذه المناسبة، ستعرض السينما أسبوعًا من الأفلام الكلاسيكية بجهازي العرض الأصليين مقاس ٣٥ ملم، إلى جانب نظام رقمي حديث للأفلام الجديدة.</p>
                <blockquote><p>«قيل لنا إن دار عرض بشاشة واحدة لا يمكنها الصمود. نحن نرى أن مكانًا بهذا الجمال سبب كافٍ للخروج»، قالت مديرة البرامج دينا فؤاد.</p></blockquote>
                """,
                "ياسمين عدلي",
                ["سينما", "تراث", "أفلام"])),

        new(
            Slug: "youth-league-expands-to-two-new-cities",
            Category: "Sports",
            Image: "youth-league-pitch.jpg",
            Gallery: [],
            DaysAgo: 11,
            Featured: false,
            FeaturedOrder: 0,
            En: new(
                "Youth League Expands to Two New Cities",
                "The Delta Youth Football League will add 48 teams in two neighbouring cities next season, giving about 900 more children aged 8 to 14 a place to play organised weekend football.",
                """
                <p>The Delta Youth Football League will expand into two neighbouring cities next season, adding 48 teams and around 900 players aged 8 to 14.</p>
                <p>The volunteer-run league, founded in 2015 with eight teams, now has more than 3,000 registered players. Every child gets equal playing time, and the league does not publish tables for its under-11 divisions.</p>
                <p>Registration fees are waived for families receiving social support, and boots and kits are collected and redistributed at the start of each season.</p>
                <p>The league is looking for 60 more volunteer coaches. Free coaching courses begin next month.</p>
                """,
                "Omar Khaled",
                ["football", "youth sport", "community"]),
            Ar: null),
    ];

    public static class SiteSettings
    {
        public sealed record Localized(
            string Name,
            (string Label, string Url)[] Navigation,
            string FooterAboutHtml,
            string SectionsColumnTitle,
            string CompanyColumnTitle,
            (string Label, string Url)[] CompanyLinks,
            string Copyright);

        public static readonly (string Platform, string Url)[] SocialLinks =
        [
            ("X", "https://x.com/deltanews"),
            ("Facebook", "https://www.facebook.com/deltanews"),
            ("Instagram", "https://www.instagram.com/deltanews"),
            ("YouTube", "https://www.youtube.com/@deltanews"),
        ];

        public static readonly Localized En = new(
            Name: "Site Settings",
            Navigation:
            [
                ("Home", "/"),
                ("News", "/news"),
                ("About", "/about-us"),
                ("Contact", "/contact-us"),
            ],
            FooterAboutHtml: "<p>Delta News is an independent, bilingual newsroom covering technology, business, sport, health and culture across the Delta region since 2016.</p>",
            SectionsColumnTitle: "Sections",
            CompanyColumnTitle: "Delta News",
            CompanyLinks:
            [
                ("About us", "/about-us"),
                ("Contact the newsroom", "/contact-us"),
            ],
            Copyright: $"© {DateTime.UtcNow.Year} Delta News. All rights reserved.");

        public static readonly Localized Ar = new(
            Name: "إعدادات الموقع",
            Navigation:
            [
                ("الرئيسية", "/"),
                ("الأخبار", "/news"),
                ("من نحن", "/about-us"),
                ("اتصل بنا", "/contact-us"),
            ],
            FooterAboutHtml: "<p>أخبار الدلتا غرفة أخبار مستقلة ثنائية اللغة تغطي التكنولوجيا والأعمال والرياضة والصحة والثقافة في إقليم الدلتا منذ عام ٢٠١٦.</p>",
            SectionsColumnTitle: "الأقسام",
            CompanyColumnTitle: "أخبار الدلتا",
            CompanyLinks:
            [
                ("من نحن", "/about-us"),
                ("تواصل مع غرفة الأخبار", "/contact-us"),
            ],
            Copyright: $"© {DateTime.UtcNow.Year} أخبار الدلتا. جميع الحقوق محفوظة.");
    }

    public static class AboutUs
    {
        public sealed record Localized(string Title, string HeroText, string BodyHtml, (string Label, string Value)[] Stats);

        public const string HeroImage = "about-newsroom.jpg";

        public static readonly Localized En = new(
            Title: "About Us",
            HeroText: "Independent news from the Delta, in English and Arabic",
            BodyHtml: """
                <p>Delta News was founded in 2016 by a group of six reporters who believed the region deserved a newsroom that was independent, careful with facts, and published in both of the languages its readers use every day.</p>
                <p>Today our team of 48 journalists, editors and photographers covers technology, business, sport, health and culture from our newsroom in Cairo and correspondents across the region.</p>
                <h2>Our standards</h2>
                <ul>
                  <li><strong>Accuracy first.</strong> Every story is checked by a second editor before it is published.</li>
                  <li><strong>Corrections in the open.</strong> When we get something wrong, we fix it and say so at the end of the article.</li>
                  <li><strong>No paid content disguised as news.</strong> Sponsored material is always clearly labelled.</li>
                </ul>
                <p>Have a story we should be covering? Get in touch through our contact page — our newsroom reads every message.</p>
                """,
            Stats:
            [
                ("Monthly readers", "1.2M"),
                ("Journalists", "48"),
                ("Years reporting", "10"),
                ("Languages", "2"),
            ]);

        public static readonly Localized Ar = new(
            Title: "من نحن",
            HeroText: "أخبار مستقلة من الدلتا باللغتين العربية والإنجليزية",
            BodyHtml: """
                <p>تأسست «أخبار الدلتا» عام ٢٠١٦ على يد ستة صحفيين آمنوا بأن الإقليم يستحق غرفة أخبار مستقلة، دقيقة في نقل الحقائق، وتنشر باللغتين اللتين يستخدمهما قراؤها كل يوم.</p>
                <p>واليوم يغطي فريقنا المكوّن من ٤٨ صحفيًا ومحررًا ومصورًا أخبار التكنولوجيا والأعمال والرياضة والصحة والثقافة من مقرنا في القاهرة ومراسلينا في أنحاء الإقليم.</p>
                <h2>معاييرنا</h2>
                <ul>
                  <li><strong>الدقة أولًا.</strong> يراجع كل خبر محرر ثانٍ قبل نشره.</li>
                  <li><strong>تصحيحات علنية.</strong> عندما نخطئ نصحح الخطأ ونشير إليه في نهاية المقال.</li>
                  <li><strong>لا محتوى مدفوعًا متخفيًا في صورة أخبار.</strong> يُميَّز المحتوى الممول دائمًا بوضوح.</li>
                </ul>
                <p>لديك قصة تستحق التغطية؟ تواصل معنا عبر صفحة الاتصال، فغرفة أخبارنا تقرأ كل رسالة.</p>
                """,
            Stats:
            [
                ("قارئ شهريًا", "١٫٢ مليون"),
                ("صحفيًا", "٤٨"),
                ("سنوات من العمل الصحفي", "١٠"),
                ("لغتان", "٢"),
            ]);
    }

    public static class ContactUs
    {
        public sealed record Localized(string Title, string Intro, string Address, string OfficeHours);

        // Reserved/fictional values: .example is an IANA-reserved TLD, so the address can never
        // reach a real inbox.
        public const string Phone = "+20 2 2795 0100";
        public const string Email = "newsroom@deltanews.example";
        public const string MapCoordinates = "30.0444,31.2357";

        public static readonly Localized En = new(
            Title: "Contact Us",
            Intro: "Questions, story tips or corrections? Our newsroom reads every message and replies within two working days.",
            Address: "6th Floor, Delta House, 22 River Street, Cairo",
            OfficeHours: "Sunday to Thursday, 9:00 am – 6:00 pm");

        public static readonly Localized Ar = new(
            Title: "اتصل بنا",
            Intro: "لديك سؤال أو معلومة لقصة أو تصحيح؟ تقرأ غرفة أخبارنا كل رسالة وترد خلال يومي عمل.",
            Address: "الطابق السادس، مبنى دلتا هاوس، ٢٢ شارع النهر، القاهرة",
            OfficeHours: "من الأحد إلى الخميس، ٩:٠٠ ص – ٦:٠٠ م");
    }
}
