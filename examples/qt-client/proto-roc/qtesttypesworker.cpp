#include "qtesttypesworker.h"
#include "gen.pb.h"
#include "gen.grpc.pb.h"
#include "roc-lib/qrocobjectadapter.h"
#include "protobuf-qjson/protobufjsonconverter.h"
#include <QThread>
using namespace Tests;
class EventThread : public QThread
{
public:
	std::shared_ptr<Tests::TestTypesObjectService::Stub> service;
	grpc::ClientContext context;
	google::protobuf::uint64 objectId;
	QTestTypesWorker* worker;
	void run() override
	{
		if(!service) { return; }
		Tests::TestTypesListenEventStream request;
		request.set_objectid(objectId);
		auto stream = service->ListenEvents(&context, request);
		google::protobuf::Any any;
		while(stream->Read(&any)) { worker->processEvent(&any); }
		auto result = stream->Finish();
		if(result.error_code() == grpc::StatusCode::CANCELLED) { return; }
		if(result.ok()) { qCritical("unabled to stop stream: %s", result.error_message().c_str()); return; }
	}
};
class Tests::QTestTypesWorkerPrivate
{
public:
	QTestTypesWorkerPrivate(QTestTypesWorker* worker) : objectId(0), worker(worker) {}
	QTestTypesWorker* worker;
	google::protobuf::uint64 objectId;
	std::shared_ptr<Tests::TestTypesObjectService::Stub> service;
	grpc::ClientContext objectRequestContext;
	std::unique_ptr<grpc::ClientReader<Tests::TestTypesCreateResponse>> objectRequest;
	EventThread eventThread;
	void createObject()
	{
		Tests::TestTypesCreateRequest request;
		objectRequest = service->Create(&objectRequestContext, request);
		Tests::TestTypesCreateResponse response;
		if(!objectRequest->Read(&response)) { qCritical(""); objectRequest.release(); return; }
		objectId = response.objectid();
		eventThread.objectId = objectId;
		eventThread.service = service;
		eventThread.worker = worker;
		eventThread.start();
	}
	void releaseObject()
	{
		if(objectRequest != nullptr)
		{
			eventThread.context.TryCancel();
			eventThread.quit();
			eventThread.wait();
			objectRequestContext.TryCancel();
			auto result = objectRequest->Finish();
			if(result.error_code() == grpc::StatusCode::CANCELLED) { return; }
			if(!result.ok()) { qCritical("Couldn't release object: %s", result.error_message().c_str()); }
		}
	}
	bool isValid()
	{
		return objectRequest != nullptr;
	}
};
QTestTypesWorker::QTestTypesWorker() : QObject(nullptr), d_priv(new QTestTypesWorkerPrivate(this))
{
	auto channel = QRocObjectAdapter::getSharedChannel();
	if(channel == nullptr) { qWarning("Set the channel to use via QRocObjectAdapter::setSharedChannel(...)"); return; }
	d_priv->service = Tests::TestTypesObjectService::NewStub(channel);
	d_priv->createObject();
}
QTestTypesWorker::~QTestTypesWorker()
{
	d_priv->releaseObject();
}
void QTestTypesWorker::testParamDouble(bool val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamDoubleMethodRequest request;
		Tests::TestTypesTestParamDoubleMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamDouble(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testParamDoubleDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamDoubleDone(responseValue, requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamFloat(bool val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamFloatMethodRequest request;
		Tests::TestTypesTestParamFloatMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamFloat(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testParamFloatDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamFloatDone(responseValue, requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamInt(bool val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamIntMethodRequest request;
		Tests::TestTypesTestParamIntMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamInt(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testParamIntDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamIntDone(responseValue, requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamUInt(bool val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamUIntMethodRequest request;
		Tests::TestTypesTestParamUIntMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamUInt(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testParamUIntDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamUIntDone(responseValue, requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamLong(bool val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamLongMethodRequest request;
		Tests::TestTypesTestParamLongMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamLong(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testParamLongDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamLongDone(responseValue, requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamULong(bool val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamULongMethodRequest request;
		Tests::TestTypesTestParamULongMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamULong(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testParamULongDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamULongDone(responseValue, requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamBool(bool val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamBoolMethodRequest request;
		Tests::TestTypesTestParamBoolMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamBool(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testParamBoolDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamBoolDone(responseValue, requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamString(QVariant val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamStringMethodRequest request;
		Tests::TestTypesTestParamStringMethodResponse response;
		request.set_objectid(d_priv->objectId);
		if(val.userType() == QMetaType::QString)
		{
			auto messageVal = new google::protobuf::StringValue();
			messageVal->set_value(val.toString().toStdString());
			request.set_allocated_value(messageVal);
		}
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamString(&context, request, &response);
		QVariant responseValue = QVariant::fromValue(nullptr);
		if(response.has_value())
		{
			responseValue = QString::fromStdString(response.value().value());
		}
		if(!invokeResult.ok()) {
			emit testParamStringDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamStringDone(responseValue, requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamByte(bool val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamByteMethodRequest request;
		Tests::TestTypesTestParamByteMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamByte(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testParamByteDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamByteDone(responseValue, requestId, QString());
		}
	});
}
void QTestTypesWorker::testParamBytes(QByteArray val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::TestTypesTestParamBytesMethodRequest request;
		Tests::TestTypesTestParamBytesMethodResponse response;
		request.set_objectid(d_priv->objectId);
		if(val.isNull())
		{
			auto messageVal = new google::protobuf::BytesValue();
			messageVal->set_value(val);
			request.set_allocated_value(messageVal);
		}
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestParamBytes(&context, request, &response);
		QByteArray responseValue;
		if(response.has_value())
		{
			responseValue = QByteArray::fromStdString(response.value().value());
		}
		if(!invokeResult.ok()) {
			emit testParamBytesDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testParamBytesDone(responseValue, requestId, QString());
		}
	});
}
void QTestTypesWorker::processEvent(void* _event)
{
	auto event = reinterpret_cast<google::protobuf::Any*>(_event);
	if(event->Is<Tests::TestTypesTestEventEvent>())
	{
		Tests::TestTypesTestEventEvent eventMessage;
		event->UnpackTo(&eventMessage);
		QVariant eventValue = QVariant::fromValue(nullptr);
		if(eventMessage.has_value())
		{
			eventValue = QString::fromStdString(eventMessage.value().value());
		}
		emit testEventRaised(eventValue);
	}
	qDebug("got event: %s", event->type_url().c_str());
}
