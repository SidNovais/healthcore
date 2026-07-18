import { Pipe, PipeTransform } from '@angular/core';

/** Placeholder shown when there is no timestamp to render. */
const PLACEHOLDER = '—';

/** A trailing `Z` or `±hh[:]mm` offset — i.e. the string already states its zone. */
const HAS_ZONE = /(Z|[+-]\d{2}:?\d{2})$/;

/**
 * Formats an instant (e.g. registeredAt, requestedAt, createdAt) as a
 * human-readable date and time in the viewer's browser locale and local zone.
 *
 * The backend contract is UTC. If a value arrives without a `Z`/offset we append
 * `Z` so it is interpreted as UTC rather than as the viewer's local time — this
 * keeps rendering correct whether or not the wire format carries the marker.
 */
@Pipe({ name: 'hcDateTime' })
export class HcDateTimePipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) {
      return PLACEHOLDER;
    }
    const normalized = HAS_ZONE.test(value) ? value : `${value}Z`;
    const date = new Date(normalized);
    if (Number.isNaN(date.getTime())) {
      return PLACEHOLDER;
    }
    return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(
      date,
    );
  }
}
