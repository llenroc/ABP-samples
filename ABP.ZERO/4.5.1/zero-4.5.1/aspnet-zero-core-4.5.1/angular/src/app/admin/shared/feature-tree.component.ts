import { Component, OnInit, AfterViewInit, AfterViewChecked, ElementRef, ViewChild, Injector, Input, Output, EventEmitter } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { NameValueDto, FlatFeatureDto } from '@shared/service-proxies/service-proxies';
import { FeatureTreeEditModel } from '@app/admin/shared/feature-tree-edit.model';
import * as _ from "lodash";

@Component({
    selector: 'feature-tree',
    template: `<div class="feature-tree"></div>`,
    styleUrls: ['./feature-tree.component.less']
})
export class FeatureTreeComponent extends AppComponentBase implements OnInit, AfterViewInit, AfterViewChecked {

    set editData(val: FeatureTreeEditModel) {
        this._editData = val;
        this.refreshTree();
    }

    private _$tree: JQuery;
    private _editData: FeatureTreeEditModel;
    private _createdTreeBefore;

    constructor(private _element: ElementRef,
        injector: Injector
    ) {
        super(injector);
    }

    ngOnInit(): void {

    }

    ngAfterViewInit(): void {
        this._$tree = $(this._element.nativeElement);

        this.refreshTree();
    }

    ngAfterViewChecked(): void {

    }

    getGrantedFeatures(): NameValueDto[] {
        if (!this._$tree || !this._createdTreeBefore) {
            return [];
        }

        var selectedFeatures = this._$tree.jstree('get_selected', true);

        return _.map(this._editData.features, item => {
            let feature = new NameValueDto();

            feature.name = item.name;

            if (!item.inputType || item.inputType.name == 'CHECKBOX') {
                feature.value = _.some(selectedFeatures, { original: { id: item.name } }) ? "true" : "false";
            } else {
                feature.value = this.getFeatureValueByName(item.name);
            }

            return feature;
        });
    }

    refreshTree(): void {
        var self = this;

        if (this._createdTreeBefore) {
            this._$tree.jstree('destroy');
        }

        this._createdTreeBefore = false;

        if (!this._editData || !this._$tree) {
            return;
        }

        var treeData = _.map(this._editData.features, function (item) {
            return {
                id: item.name,
                parent: item.parentName ? item.parentName : '#',
                text: item.displayName,
                state: {
                    opened: true,
                    selected: _.some(self._editData.featureValues, { name: item.name, value: "true" })
                }
            };
        });

        this._$tree
            .on('ready.jstree', function () {
                self.customizeTreeNodes();
            })
            .on('redraw.jstree', function () {
                self.customizeTreeNodes();
            })
            .on('after_open.jstree', function () {
                self.customizeTreeNodes();
            })
            .on('create_node.jstree', function () {
                self.customizeTreeNodes();
            })
            .on("changed.jstree", function (e, data) {
                if (!data.node) {
                    return;
                }

                var wasInTreeChangeEvent = inTreeChangeEvent;
                if (!wasInTreeChangeEvent) {
                    inTreeChangeEvent = true;
                }

                var childrenNodes;

                if (data.node.state.selected) {
                    selectNodeAndAllParents(self._$tree.jstree('get_parent', data.node));

                    childrenNodes = $.makeArray(self._$tree.jstree('get_children_dom', data.node));
                    self._$tree.jstree('select_node', childrenNodes);

                } else {
                    childrenNodes = $.makeArray(self._$tree.jstree('get_children_dom', data.node));
                    self._$tree.jstree('deselect_node', childrenNodes);
                }

                if (!wasInTreeChangeEvent) {
                    var $nodeLi = self.getNodeLiByFeatureName(data.node.id);
                    var feature = self.findFeatureByName(data.node.id);
                    if (feature && (!feature.inputType || feature.inputType.name == 'CHECKBOX')) {
                        var value = self._$tree.jstree('is_checked', $nodeLi) ? 'true' : 'false';
                        self.setFeatureValueByName(data.node.id, value);
                    }

                    inTreeChangeEvent = false;
                }
            })
            .jstree({
                'core': {
                    data: treeData
                },
                "types": {
                    "default": {
                        "icon": "fa fa-folder tree-item-icon-color icon-lg"
                    },
                    "file": {
                        "icon": "fa fa-file tree-item-icon-color icon-lg"
                    }
                },
                'checkbox': {
                    keep_selected_style: false,
                    three_state: false,
                    cascade: ''
                },
                plugins: ['checkbox', 'types']
            });

        this._createdTreeBefore = true;

        let inTreeChangeEvent = false;

        function selectNodeAndAllParents(node) {
            self._$tree.jstree('select_node', node, true);
            var parent = self._$tree.jstree('get_parent', node);
            if (parent) {
                selectNodeAndAllParents(parent);
            }
        };

        this._$tree.on("changed.jstree", function (e, data) {
            if (!data.node) {
                return;
            }

            var wasInTreeChangeEvent = inTreeChangeEvent;
            if (!wasInTreeChangeEvent) {
                inTreeChangeEvent = true;
            }

            var childrenNodes;

            if (data.node.state.selected) {
                selectNodeAndAllParents(self._$tree.jstree('get_parent', data.node));

                childrenNodes = $.makeArray(self._$tree.jstree('get_children_dom', data.node));
                self._$tree.jstree('select_node', childrenNodes);

            } else {
                childrenNodes = $.makeArray(self._$tree.jstree('get_children_dom', data.node));
                self._$tree.jstree('deselect_node', childrenNodes);
            }

            if (!wasInTreeChangeEvent) {
                inTreeChangeEvent = false;
            }
        });
    }

    customizeTreeNodes(): void {
        let self = this;
        self._$tree.find('.jstree-node').each(function () {
            var $nodeLi = $(this);
            var $nodeA = $nodeLi.find('.jstree-anchor');

            var featureName = $nodeLi.attr('id');
            var feature = self.findFeatureByName(featureName);
            var featureValue = self.findFeatureValueByName(featureName) || '';

            if (!feature || !feature.inputType) {
                return;
            }

            if (feature.inputType.name == 'CHECKBOX') {
                //no change for checkbox
            } else if (feature.inputType.name == 'SINGLE_LINE_STRING') {
                if (!$nodeLi.find('.feature-tree-textbox').length) {
                    $nodeA.find('.jstree-checkbox').hide();

                    var inputType = 'text';
                    let validator = (feature.inputType.validator as any);
                    if (feature.inputType.validator) {
                        if (feature.inputType.validator.name == 'NUMERIC') {
                            inputType = 'number';
                        }
                    }

                    var $textbox = $('<input class="feature-tree-textbox" type="' + inputType + '" />')
                        .val(featureValue);

                    if (inputType == 'number') {
                        $textbox.attr('min', validator.minValue);
                        $textbox.attr('max', validator.maxValue);
                    } else {
                        if (feature.inputType.validator && feature.inputType.validator.name == 'STRING') {
                            if (validator.maxLength > 0) {
                                $textbox.attr('maxlength', validator.maxLength);
                            }
                            if (validator.minLength > 0) {
                                $textbox.attr('required', 'required');
                            }
                            if (validator.regularExpression) {
                                $textbox.attr('pattern', validator.regularExpression);
                            }
                        }
                    }

                    $textbox.on('input propertychange paste', () => {
                        var value = $textbox.val() as string;
                        if (self.isFeatureValueValid(featureName, value)) {
                            self.setFeatureValueByName(featureName, value);
                            $textbox.removeClass('feature-tree-textbox-invalid');
                        } else {
                            $textbox.addClass('feature-tree-textbox-invalid');
                        }
                    });

                    $textbox.appendTo($nodeLi);
                }
            } else if (feature.inputType.name == 'COMBOBOX') {
                if (!$nodeLi.find('.feature-tree-combobox').length) {
                    $nodeA.find('.jstree-checkbox').hide();

                    var $combobox = $('<select class="feature-tree-combobox" />');
                    let inputType = (feature.inputType as any);
                    _.each(inputType.itemSource.items, function (opt: any) {
                        $('<option></option>')
                            .attr('value', opt.value)
                            .text(opt.displayText)
                            .appendTo($combobox);
                    });

                    $combobox
                        .val(featureValue)
                        .on('change', () => {
                            var value = $combobox.val() as string;
                            self.setFeatureValueByName(featureName, value);
                        })
                        .appendTo($nodeLi);
                }
            }
        });
    }

    getNodeLiByFeatureName(featureName: string): JQuery {
        return $('#' + featureName.replace('.', '\\.'));
    }

    selectNodeAndAllParents(node: any): void {
        let self = this;
        self._$tree.jstree('select_node', node, true);
        var parent = self._$tree.jstree('get_parent', node);
        if (parent) {
            self.selectNodeAndAllParents(parent);
        }
    };

    findFeatureByName(featureName: string): FlatFeatureDto {
        let self = this;

        var feature = _.find(self._editData.features, function (f) { return f.name == featureName });

        if (!feature) {
            abp.log.warn('Could not find a feature by name: ' + featureName);
        }

        return feature;
    }

    findFeatureValueByName(featureName: string) {
        let self = this;
        var feature = self.findFeatureByName(featureName);
        if (!feature) {
            return '';
        }

        var featureValue = _.find(self._editData.featureValues, function (f) { return f.name == featureName });
        if (!featureValue) {
            return feature.defaultValue;
        }

        return featureValue.value;
    }

    isFeatureValueValid(featureName: string, value: string): boolean {
        let self = this;
        var feature = self.findFeatureByName(featureName);
        if (!feature || !feature.inputType || !feature.inputType.validator) {
            return true;
        }

        var validator = (feature.inputType.validator as any);
        if (validator.name == 'STRING') {
            if (value == undefined || value == null) {
                return validator.allowNull;
            }

            if (typeof value != 'string') {
                return false;
            }

            if (validator.minLength > 0 && value.length < validator.minLength) {
                return false;
            }

            if (validator.maxLength > 0 && value.length > validator.maxLength) {
                return false;
            }

            if (validator.regularExpression) {
                return (new RegExp(validator.regularExpression)).test(value);
            }
        } else if (validator.name == 'NUMERIC') {
            var numValue = parseInt(value);

            if (isNaN(numValue)) {
                return false;
            }

            var minValue = validator.minValue;
            if (minValue > numValue) {
                return false;
            }

            var maxValue = validator.maxValue;
            if (maxValue > 0 && numValue > maxValue) {
                return false;
            }
        }

        return true;
    }

    areAllValuesValid(): boolean {
        let self = this;
        self._$tree.find('.jstree-node').each(function () {
            var $nodeLi = $(this);
            var featureName = $nodeLi.attr('id');
            var feature = self.findFeatureByName(featureName);

            if (feature && (!feature.inputType || feature.inputType.name == 'CHECKBOX')) {
                var value = self._$tree.jstree('is_checked', $nodeLi) ? 'true' : 'false';
                self.setFeatureValueByName(featureName, value);
            }
        });

        return self._$tree.find('.feature-tree-textbox-invalid').length <= 0;
    }

    setFeatureValueByName(featureName: string, value: string): void {
        var featureValue = _.find(this._editData.featureValues, f => f.name === featureName);
        if (!featureValue) {
            return;
        }

        featureValue.value = value;
    }

    getFeatureValueByName(featureName: string): string {
        var featureValue = _.find(this._editData.featureValues, f => f.name === featureName);
        if (!featureValue) {
            return null;
        }

        return featureValue.value;
    }

    isFeatureEnabled(featureName: string): boolean {
        let self = this;
        var value = self.findFeatureValueByName(featureName);
        return value.toLowerCase() === 'true';
    }
}
