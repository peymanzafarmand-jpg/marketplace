const fa = {
  common: {
    appName: "مارکت کودک و نوزاد",
    search: "جستجو",
    searchPlaceholder: "دنبال چی می‌گردی؟",
    cart: "سبد خرید",
    account: "حساب کاربری",
    home: "خانه",
    categories: "دسته‌بندی‌ها",
    wishlist: "علاقه‌مندی‌ها",
    loading: "در حال بارگذاری…",
  },
  nav: {
    desktop: ["خانه", "دسته‌بندی‌ها", "برندها", "تخفیف‌ها", "مجله"],
    mobile: [
      { label: "خانه", key: "home" },
      { label: "دسته‌بندی", key: "categories" },
      { label: "جستجو", key: "search" },
      { label: "سبد خرید", key: "cart" },
      { label: "حساب من", key: "account" },
    ],
  },
  footer: {
    about: "درباره ما",
    support: "پشتیبانی",
    terms: "قوانین و مقررات",
    rights: "تمام حقوق محفوظ است.",
  },
  errors: {
    globalTitle: "یک مشکل پیش آمد",
    globalDescription:
      "صفحه با خطا مواجه شد. می‌توانید دوباره تلاش کنید یا به صفحه اصلی برگردید.",
    retry: "تلاش دوباره",
    backHome: "بازگشت به صفحه اصلی",
    notFoundTitle: "صفحه پیدا نشد",
    notFoundDescription: "آدرسی که دنبالش بودید وجود ندارد یا جابه‌جا شده.",
  },
} as const;

export default fa;
export type Dictionary = typeof fa;
