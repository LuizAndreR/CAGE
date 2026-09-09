import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PedidoDetail } from './pedido-detail';

describe('PedidoDetail', () => {
  let component: PedidoDetail;
  let fixture: ComponentFixture<PedidoDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PedidoDetail],
    }).compileComponents();

    fixture = TestBed.createComponent(PedidoDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
