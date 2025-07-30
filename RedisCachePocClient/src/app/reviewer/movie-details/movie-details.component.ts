import {Component, inject, input, ResourceRef, signal} from '@angular/core';
import {MovieDetailsDto, MovieReviewerClient, ReviewDto} from '../../../reviewer-client';
import {Card} from 'primeng/card';
import {MessageService, PrimeTemplate} from 'primeng/api';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {RouterLink} from '@angular/router';
import {Tag} from 'primeng/tag';
import {AddReviewComponent} from '../add-review/add-review.component';
import {resourceObs} from '../../utils';
import {ProgressSpinner} from 'primeng/progressspinner';
import {Rating} from 'primeng/rating';
import {FormsModule} from '@angular/forms';
import {DatePipe} from '@angular/common';
import {StyleClass} from 'primeng/styleclass';

@Component({
  selector: 'app-movie-details',
  imports: [
    Card,
    PrimeTemplate,
    ButtonDirective,
    RouterLink,
    ButtonLabel,
    ButtonIcon,
    Tag,
    AddReviewComponent,
    ProgressSpinner,
    Rating,
    FormsModule,
    DatePipe,
    StyleClass
  ],
  templateUrl: './movie-details.component.html',
  styleUrl: './movie-details.component.css'
})
export class MovieDetailsComponent {
  private readonly client = inject(MovieReviewerClient);
  private readonly messageService = inject(MessageService);

  readonly movieId = input.required<string>()
  readonly plotExpanded = signal(false);

  protected readonly movie: ResourceRef<MovieDetailsDto> = resourceObs(() => this.movieId(), param => this.client.moviesIdGet(param));

  addReview(r: ReviewDto) {
    if (!this.movie.hasValue()) {
      return;
    }
    const current = this.movie.value();
    if (!current) return;

    this.movie.set({
      ...current,
      reviews: [...(current.reviews ?? []), r]
    });
  }

  generateReviews() {
    this.client.generateReviewsForMovie(this.movieId(), 10).subscribe(r => {
      this.movie.reload();
      this.messageService.add({
        severity: 'success',
        summary: 'Reviews generated',
        detail: `Generated ${r.length} reviews for movie '${this.movie.value()?.title}'!`,

      });
    });
  }

  togglePlot() {
    this.plotExpanded.set(!this.plotExpanded());
  }

  getPlotPreview(fullPlot: string, sentenceCount = 2): string {
    const sentences = fullPlot.match(/[^\.!\?]+[\.!\?]+/g) || [fullPlot];
    const preview = sentences.slice(0, sentenceCount).join(' ').trim();
    return preview.endsWith('.') ? preview : preview + '…';
  }
}
