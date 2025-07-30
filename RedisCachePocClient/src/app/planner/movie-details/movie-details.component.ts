import {Component, inject, input, OnInit, signal} from '@angular/core';
import {MovieDetailsDto, MovieReviewerClient, ReviewDto} from '../../../reviewer-client';
import {MessageService, PrimeTemplate} from 'primeng/api';
import {MoviePlannerClient} from '../../../planner-client';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {Card} from 'primeng/card';
import {Tag} from 'primeng/tag';
import {RouterLink} from '@angular/router';
import {NgClass} from '@angular/common';

@Component({
  selector: 'app-movie-details',
  imports: [
    ButtonDirective,
    ButtonIcon,
    ButtonLabel,
    Card,
    PrimeTemplate,
    Tag,
    RouterLink,
  ],
  templateUrl: './movie-details.component.html',
  styleUrl: './movie-details.component.css'
})
export class MovieDetailsComponent implements OnInit {
  private readonly client = inject(MoviePlannerClient);
  readonly plotExpanded = signal(false);

  readonly movieId = input.required<string>()
  protected readonly movie = signal<MovieDetailsDto | null>(null);

  ngOnInit(): void {
    this.reload();
  }
  reload() {
    this.client.getMovie(this.movieId()).subscribe(movie => this.movie.set(movie));
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
