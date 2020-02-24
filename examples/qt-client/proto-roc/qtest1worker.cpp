#include "qtest1worker.h"
#include "gen.pb.h"
#include "gen.grpc.pb.h"
#include "roc-lib/qrocobjectadapter.h"
#include "protobuf-qjson/protobufjsonconverter.h"
#include <QThread>
using namespace Tests;
class EventThread : public QThread
{
public:
	std::shared_ptr<Tests::Test1ObjectService::Stub> service;
	grpc::ClientContext context;
	google::protobuf::uint64 objectId;
	QTest1Worker* worker;
	void run() override
	{
		if(!service) { return; }
		Tests::Test1ListenEventStream request;
		request.set_objectid(objectId);
		auto stream = service->ListenEvents(&context, request);
		google::protobuf::Any any;
		while(stream->Read(&any)) { worker->processEvent(&any); }
		auto result = stream->Finish();
		if(result.error_code() == grpc::StatusCode::CANCELLED) { return; }
		if(result.ok()) { qCritical("unabled to stop stream: %s", result.error_message().c_str()); return; }
	}
};
class Tests::QTest1WorkerPrivate
{
public:
	QTest1WorkerPrivate(QTest1Worker* worker) : objectId(0), worker(worker) {}
	QTest1Worker* worker;
	google::protobuf::uint64 objectId;
	std::shared_ptr<Tests::Test1ObjectService::Stub> service;
	grpc::ClientContext objectRequestContext;
	std::unique_ptr<grpc::ClientReader<Tests::Test1CreateResponse>> objectRequest;
	EventThread eventThread;
	void createObject()
	{
		Tests::Test1CreateRequest request;
		objectRequest = service->Create(&objectRequestContext, request);
		Tests::Test1CreateResponse response;
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
QTest1Worker::QTest1Worker() : QObject(nullptr), d_priv(new QTest1WorkerPrivate(this))
{
	auto channel = QRocObjectAdapter::getSharedChannel();
	if(channel == nullptr) { qWarning("Set the channel to use via QRocObjectAdapter::setSharedChannel(...)"); return; }
	d_priv->service = Tests::Test1ObjectService::NewStub(channel);
	d_priv->createObject();
}
QTest1Worker::~QTest1Worker()
{
	d_priv->releaseObject();
}
void QTest1Worker::testMethod(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test1TestMethodMethodRequest request;
		Tests::Test1TestMethodMethodResponse response;
		request.set_objectid(d_priv->objectId);
		if(!val.isNull())
		{
			auto messageVal = new custom::types::TestMessageRequest();
			ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
			request.set_allocated_value(messageVal);
		}
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethod(&context, request, &response);
		QJsonValue responseValue;
		if(response.has_value())
		{
			auto responseMessageValue = response.value();
			ProtobufJsonConverter::messageToJsonValue(&responseMessageValue, responseValue);
		}
		if(!invokeResult.ok()) {
			emit testMethodDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodDone(responseValue, requestId, QString());
		}
	});
}
void QTest1Worker::testMethodSync(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test1TestMethodSyncMethodRequest request;
		Tests::Test1TestMethodSyncMethodResponse response;
		request.set_objectid(d_priv->objectId);
		if(!val.isNull())
		{
			auto messageVal = new custom::types::TestMessageRequest();
			ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
			request.set_allocated_value(messageVal);
		}
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodSync(&context, request, &response);
		QJsonValue responseValue;
		if(response.has_value())
		{
			auto responseMessageValue = response.value();
			ProtobufJsonConverter::messageToJsonValue(&responseMessageValue, responseValue);
		}
		if(!invokeResult.ok()) {
			emit testMethodSyncDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodSyncDone(responseValue, requestId, QString());
		}
	});
}
void QTest1Worker::testMethodWithNoResponse(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test1TestMethodWithNoResponseMethodRequest request;
		Tests::Test1TestMethodWithNoResponseMethodResponse response;
		request.set_objectid(d_priv->objectId);
		if(!val.isNull())
		{
			auto messageVal = new custom::types::TestMessageRequest();
			ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
			request.set_allocated_value(messageVal);
		}
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodWithNoResponse(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testMethodWithNoResponseDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodWithNoResponseDone(requestId, QString());
		}
	});
}
void QTest1Worker::testMethodPrimitive(bool val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test1TestMethodPrimitiveMethodRequest request;
		Tests::Test1TestMethodPrimitiveMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodPrimitive(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testMethodPrimitiveDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodPrimitiveDone(responseValue, requestId, QString());
		}
	});
}
void QTest1Worker::testMethodNoRequestOrResponse(int requestId)
{
	QMetaObject::invokeMethod(this, [this, requestId] {
		Tests::Test1TestMethodNoRequestOrResponseMethodRequest request;
		Tests::Test1TestMethodNoRequestOrResponseMethodResponse response;
		request.set_objectid(d_priv->objectId);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodNoRequestOrResponse(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testMethodNoRequestOrResponseDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodNoRequestOrResponseDone(requestId, QString());
		}
	});
}
void QTest1Worker::testMethodNoRequest(int requestId)
{
	QMetaObject::invokeMethod(this, [this, requestId] {
		Tests::Test1TestMethodNoRequestMethodRequest request;
		Tests::Test1TestMethodNoRequestMethodResponse response;
		request.set_objectid(d_priv->objectId);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodNoRequest(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testMethodNoRequestDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodNoRequestDone(responseValue, requestId, QString());
		}
	});
}
void QTest1Worker::testMethodNoResponse(bool val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test1TestMethodNoResponseMethodRequest request;
		Tests::Test1TestMethodNoResponseMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodNoResponse(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testMethodNoResponseDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodNoResponseDone(requestId, QString());
		}
	});
}
void QTest1Worker::processEvent(void* _event)
{
	auto event = reinterpret_cast<google::protobuf::Any*>(_event);
	if(event->Is<Tests::Test1TestEventEvent>())
	{
		Tests::Test1TestEventEvent eventMessage;
		event->UnpackTo(&eventMessage);
		QVariant eventValue = QVariant::fromValue(nullptr);
		if(eventMessage.has_value())
		{
			eventValue = QString::fromStdString(eventMessage.value().value());
		}
		emit testEventRaised(eventValue);
	}
	if(event->Is<Tests::Test1TestEventComplexEvent>())
	{
		Tests::Test1TestEventComplexEvent eventMessage;
		event->UnpackTo(&eventMessage);
		QJsonValue eventValue;
		if(eventMessage.has_value())
		{
			auto eventMessageMessageValue = eventMessage.value();
			ProtobufJsonConverter::messageToJsonValue(&eventMessageMessageValue, eventValue);
		}
		emit testEventComplexRaised(eventValue);
	}
	if(event->Is<Tests::Test1TestEventNoDataEvent>())
	{
		emit testEventNoDataRaised();
	}
	if(event->Is<Tests::Test1PropStringPropertyChanged>())
	{
		Tests::Test1PropStringPropertyChanged eventMessage;
		event->UnpackTo(&eventMessage);
		QVariant eventValue = QVariant::fromValue(nullptr);
		if(eventMessage.has_value())
		{
			eventValue = QString::fromStdString(eventMessage.value().value());
		}
		emit propStringChanged(eventValue);
	}
	if(event->Is<Tests::Test1PropComplexPropertyChanged>())
	{
		Tests::Test1PropComplexPropertyChanged eventMessage;
		event->UnpackTo(&eventMessage);
		QJsonValue eventValue;
		if(eventMessage.has_value())
		{
			auto eventMessageMessageValue = eventMessage.value();
			ProtobufJsonConverter::messageToJsonValue(&eventMessageMessageValue, eventValue);
		}
		emit propComplexChanged(eventValue);
	}
	qDebug("got event: %s", event->type_url().c_str());
}
QVariant QTest1Worker::getPropString()
{
	Tests::Test1PropStringGetRequest request;
	Tests::Test1PropStringGetResponse response;
	request.set_objectid(d_priv->objectId);
	grpc::ClientContext context;
	auto result = d_priv->service->GetPropertyPropString(&context, request, &response);
	if(!result.ok()) { qCritical("couldn't read property PropString: %s", result.error_message().c_str()); return QVariant(); }
	QVariant propValue = QVariant::fromValue(nullptr);
	if(response.has_value())
	{
		propValue = QString::fromStdString(response.value().value());
	}
	return propValue;
}
void QTest1Worker::setPropString(QVariant val)
{
	Tests::Test1PropStringSetRequest request;
	Tests::Test1PropStringSetResponse response;
	request.set_objectid(d_priv->objectId);
	if(val.userType() == QMetaType::QString)
	{
		auto messageVal = new google::protobuf::StringValue();
		messageVal->set_value(val.toString().toStdString());
		request.set_allocated_value(messageVal);
	}
	grpc::ClientContext context;
	auto result = d_priv->service->SetPropertyPropString(&context, request, &response);
	if(!result.ok()) { qCritical("couldn't set property propString: %s", result.error_message().c_str()); }
}
QJsonValue QTest1Worker::getPropComplex()
{
	Tests::Test1PropComplexGetRequest request;
	Tests::Test1PropComplexGetResponse response;
	request.set_objectid(d_priv->objectId);
	grpc::ClientContext context;
	auto result = d_priv->service->GetPropertyPropComplex(&context, request, &response);
	if(!result.ok()) { qCritical("couldn't read property PropComplex: %s", result.error_message().c_str()); return QJsonValue::Undefined; }
	QJsonValue propValue;
	if(response.has_value())
	{
		auto responseMessageValue = response.value();
		ProtobufJsonConverter::messageToJsonValue(&responseMessageValue, propValue);
	}
	return propValue;
}
void QTest1Worker::setPropComplex(QJsonValue val)
{
	Tests::Test1PropComplexSetRequest request;
	Tests::Test1PropComplexSetResponse response;
	request.set_objectid(d_priv->objectId);
	if(!val.isNull())
	{
		auto messageVal = new custom::types::TestMessageResponse();
		ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
		request.set_allocated_value(messageVal);
	}
	grpc::ClientContext context;
	auto result = d_priv->service->SetPropertyPropComplex(&context, request, &response);
	if(!result.ok()) { qCritical("couldn't set property propComplex: %s", result.error_message().c_str()); }
}
