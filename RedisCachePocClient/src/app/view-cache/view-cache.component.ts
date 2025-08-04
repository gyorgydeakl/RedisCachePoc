import {Component, computed, effect, inject} from '@angular/core';
import {MovieReviewerClient} from '../../reviewer-client';
import {resourceObs, resourceObsNoParams} from '../utils';
import {ProgressSpinner} from 'primeng/progressspinner';
import {NgxJsonViewerModule} from 'ngx-json-viewer';
import {ScrollPanel} from 'primeng/scrollpanel';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {MessageService, PrimeTemplate} from 'primeng/api';
import {DynamicDialogConfig} from 'primeng/dynamicdialog';

@Component({
  selector: 'app-view-cache',
  imports: [
    ProgressSpinner,
    NgxJsonViewerModule,
    ScrollPanel,
    ButtonDirective,
    ButtonIcon,
    ButtonLabel,
    PrimeTemplate
  ],
  templateUrl: './view-cache.component.html',
  styleUrl: './view-cache.component.css'
})
export class ViewCacheComponent {
  private readonly client = inject(MovieReviewerClient);
  private readonly messageService = inject(MessageService);

  readonly data = resourceObsNoParams(() => this.client.getFullCache())
  readonly isDataEmpty = computed<boolean>(() => Object.keys(this.data.value()).length === 0);

  constructor() {
    effect(() => console.log(this.data.value()));
  }
  clearCache() {
    this.client.clearCache().subscribe(
      _ => {
        this.messageService.add({
          severity: 'success',
          summary: 'Cache cleared',
        });

        this.data.reload();
      }
    );
  }

}
