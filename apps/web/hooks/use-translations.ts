import { defaultLocale } from "@/lib/i18n/config";
import fa, { type Dictionary } from "@/lib/i18n/dictionaries/fa";

const dictionaries: Record<string, Dictionary> = {
  fa,
};

/**
 * Returns the dictionary for the active locale. Client and server
 * components can both call this. Once real locale switching exists,
 * this reads the locale from route params/cookie instead of the constant.
 */
export function useTranslations(): Dictionary {
  return dictionaries[defaultLocale];
}
