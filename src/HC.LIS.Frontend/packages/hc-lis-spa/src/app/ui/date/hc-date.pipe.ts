import { Pipe, PipeTransform } from '@angular/core';

/** Placeholder shown when there is no date to render. */
const PLACEHOLDER = '—';

/** Leading `YYYY-MM-DD` of an ISO string; any time/zone suffix is ignored. */
const ISO_DATE = /^(\d{4})-(\d{2})-(\d{2})/;

/**
 * Formats a calendar date (e.g. date of birth) in the viewer's browser locale.
 *
 * The value is treated as Y/M/D parts, never as an instant: the backend types
 * these as UTC-midnight `DateTime`s, so letting `Date` parse the raw ISO string
 * would shift the day back for any viewer west of UTC. We pin `timeZone: 'UTC'`
 * against a UTC-built date so the rendered day always equals the stored day.
 */
@Pipe({ name: 'hcDate' })
export class HcDatePipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) {
      return PLACEHOLDER;
    }
    const match = ISO_DATE.exec(value);
    if (!match) {
      return PLACEHOLDER;
    }
    const [y, m, d] = [Number(match[1]), Number(match[2]), Number(match[3])];
    const date = new Date(Date.UTC(y, m - 1, d));
    const isReal =
      date.getUTCFullYear() === y && date.getUTCMonth() === m - 1 && date.getUTCDate() === d;
    if (!isReal) {
      return PLACEHOLDER;
    }
    return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeZone: 'UTC' }).format(date);
  }
}
