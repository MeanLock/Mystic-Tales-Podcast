declare module "sanitize-html" {
  interface SanitizeOptions {
    allowedTags?: string[] | false;
    allowedAttributes?: Record<string, string[]> | false;
    allowedSchemes?: string[];
    allowProtocolRelative?: boolean;
  }

  export default function sanitizeHtml(
    dirty: string,
    options?: SanitizeOptions
  ): string;
}
