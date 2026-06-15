import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EventSummary } from '../../../features/events/models/event-summary.model';

const GRADIENTS: Record<string, string> = {
  'Tech':          'linear-gradient(135deg,#667eea,#764ba2)',
  'Technology':    'linear-gradient(135deg,#667eea,#764ba2)',
  'Music':         'linear-gradient(135deg,#f093fb,#f5576c)',
  'Business':      'linear-gradient(135deg,#4facfe,#00f2fe)',
  'Sports':        'linear-gradient(135deg,#43e97b,#38f9d7)',
  'Art & Culture': 'linear-gradient(135deg,#fa709a,#fee140)',
  'Art':           'linear-gradient(135deg,#fa709a,#fee140)',
  'Education':     'linear-gradient(135deg,#a18cd1,#fbc2eb)',
  'Health':        'linear-gradient(135deg,#84fab0,#8fd3f4)',
  'Food':          'linear-gradient(135deg,#f6d365,#fda085)',
  'Photography':   'linear-gradient(135deg,#e0c3fc,#8ec5fc)',
  'Conference':    'linear-gradient(135deg,#f7971e,#ffd200)',
  'Workshop':      'linear-gradient(135deg,#0ba360,#3cba92)',
};

export function getCategoryGradient(category: string): string {
  return GRADIENTS[category] ?? 'linear-gradient(135deg,#7c3aed,#4f46e5)';
}

@Component({
  selector: 'app-event-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './event-card.component.html',
  styleUrl: './event-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventCardComponent {
  @Input({ required: true }) event!: EventSummary;

  get gradient(): string {
    return getCategoryGradient(this.event.categoryName);
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric'
    });
  }

  formatTime(iso: string): string {
    return new Date(iso).toLocaleTimeString('en-US', {
      hour: 'numeric', minute: '2-digit'
    });
  }
}